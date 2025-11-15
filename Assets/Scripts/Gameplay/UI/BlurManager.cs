using CustomLibrary.References;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurManager : MonoBehaviour
{
    public static BlurManager Instance;

    public enum BlurMethod { VolumeWeight, DepthOfFieldBokeh }

    [Header("General")]
    [SerializeField] private Volume volume;
    [SerializeField] private BlurMethod method = BlurMethod.VolumeWeight;
    [SerializeField] private float defaultDuration = 0.25f;

    [Header("Tweening")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1);

    private Coroutine _activeRoutine;
    private bool _currentTarget;


    void Awake()
    {
        Initializer.SetInstance(this);
        if (volume == null)
            volume = GetComponent<Volume>();
    }

    public void SetBlur(bool enable)
    {
        SetBlur(enable, defaultDuration);
    }

    public void SetBlur(bool enable, float duration)
    {
        _currentTarget = enable;

        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        _activeRoutine = StartCoroutine(CoSetBlur(enable, Mathf.Max(0f, duration)));
    }

    private IEnumerator CoSetBlur(bool enable, float duration)
    {
        if (volume == null)
            yield break;

        float t = 0f;
        float dt() => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        switch (method)
        {
            case BlurMethod.VolumeWeight:
                {
                    float start = volume.weight;

                    float end = enable ? 1f : 0.65f;

                    if (duration <= 0f)
                    {
                        volume.weight = end;
                        yield break;
                    }

                    while (t < duration)
                    {
                        t += dt();
                        float lerp = t / duration;
                        if (ease != null && ease.keys != null && ease.keys.Length > 1)
                            lerp = Mathf.Clamp01(ease.Evaluate(lerp));
                        else
                            lerp = Mathf.Clamp01(lerp);

                        volume.weight = Mathf.Lerp(start, end, lerp);
                        yield return null;
                    }

                    volume.weight = end;
                    break;
                }

            case BlurMethod.DepthOfFieldBokeh:
                Debug.LogWarning("DepthOfFieldBokeh not active in this trimmed version.");
                break;
        }

        _activeRoutine = null;
    }
}
