Shader "UI/URP Blur Behind"
{
    Properties
    {
        // Standard UI tint
        _Color ("Tint", Color) = (1,1,1,1)

        // Blur controls
        _BlurRadius ("Blur Radius (px)", Range(0, 8)) = 3
        _Iterations ("Iterations", Range(1, 4)) = 2
        _Downsample ("Downsample", Range(1, 4)) = 1

        // Needed for UI masking (RectMask2D/Mask)
        [PerRendererData]_MainTex ("Sprite", 2D) = "white" {} // not used for sampling, but keeps UI pipeline happy
        [PerRendererData]_StencilComp ("Stencil Comparison", Float) = 8
        [PerRendererData]_Stencil ("Stencil ID", Float) = 0
        [PerRendererData]_StencilOp ("Stencil Operation", Float) = 0
        [PerRendererData]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [PerRendererData]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [PerRendererData]_ColorMask ("Color Mask", Float) = 15
        [PerRendererData]_UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [PerRendererData]_ClipRect ("Clip Rect", Vector) = ( -32767, -32767, 32767, 32767 )
    }

    SubShader
    {
        Tags{
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UI Blur Behind"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            #ifndef fixed
                #define fixed   float
                #define fixed2  float2
                #define fixed3  float3
                #define fixed4  float4
            #endif
            #include "UnityUI.cginc"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // UI clipping helpers

            // Camera opaque texture (URP must provide this)
            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            // Dummy to keep UI batching happy (not used for blur sampling)
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ClipRect;
                float _BlurRadius;
                float _Iterations;
                float _Downsample;
            CBUFFER_END

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float4 color    : COLOR;
            };

            struct v2f
            {
                float4 posCS      : SV_POSITION;
                float4 screenPos  : TEXCOORD0; // for screen UV
                float2 uv         : TEXCOORD1; // kept for UI alpha clip compatibility
                float4 color      : COLOR;
                float2 localPos   : TEXCOORD2; // for RectMask2D clipping
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.posCS = TransformObjectToHClip(v.vertex.xyz);
                o.screenPos = ComputeScreenPos(o.posCS);
                o.uv = v.uv;
                o.color = v.color;
                o.localPos = v.vertex.xy;
                return o;
            }

            // Multi-tap blur helper (Kawase-ish)
            float4 SampleBlur(float2 uv, float2 texel, float radius, int iterations)
            {
                // Start from the base color
                float4 col = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
                float totalW = 1.0;

                // 8-tap ring per iteration
                const float2 OFF[8] = {
                    float2( 1,  0), float2(-1,  0),
                    float2( 0,  1), float2( 0, -1),
                    float2( 1,  1), float2(-1,  1),
                    float2( 1, -1), float2(-1, -1)
                };

                for (int k = 0; k < iterations; k++)
                {
                    float step = radius * (k + 1);
                    for (int i = 0; i < 8; i++)
                    {
                        float2 offs = OFF[i] * texel * step;
                        float4 s = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + offs);
                        col += s;
                        totalW += 1.0;
                    }
                }

                return col / totalW;
            }

            // UI alpha clip helper (respects RectMask2D)
            float4 ApplyClipping(float4 c, float2 localPos)
            {
                #ifdef UNITY_UI_CLIP_RECT
                    float2 clip = UnityGet2DClipping(localPos, _ClipRect);
                    c.a *= clip.x * clip.y;
                #endif
                return c;
            }

            float4 frag (v2f i) : SV_Target
            {
                // Screen UV from clip-space
                float2 uv = (i.screenPos.xy / i.screenPos.w) / _ScaledScreenParams.xy;

                // Effective texel size (allow downsampling for speed/stronger blur)
                float2 texel = (1.0 / _ScaledScreenParams.xy) * _Downsample;

                // Blur the camera color
                int iters = (int)round(saturate(_Iterations) * 4.0); // 1..4
                iters = max(1, min(iters, 4));

                float radius = _BlurRadius; // in pixels at 1x scale

                float4 blurred = SampleBlur(uv, texel, radius, iters);

                // Tint & UI vertex color (so CanvasGroup, Button states, etc. work)
                float4 outCol = blurred * _Color * i.color;

                // Respect RectMask2D/Mask
                outCol = ApplyClipping(outCol, i.localPos);

                // Optional: alpha clip for UI (rarely used here)
                #ifdef UNITY_UI_ALPHACLIP
                    clip(outCol.a - 0.001);
                #endif

                return outCol;
            }
            ENDHLSL
        }
    }

    FallBack Off
}
