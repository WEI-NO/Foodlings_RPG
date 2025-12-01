using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Sorts a SpriteRenderer by Y so that higher Y positions render behind lower ones.
/// Supports a custom "pivot" reference point for sorting and draws a gizmo at that point.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class LayerSorter : MonoBehaviour
{
    [Header("Sorting")]
    [Tooltip("Renderer to sort. Defaults to the SpriteRenderer on this GameObject.")]
    [SerializeField] private SpriteRenderer targetRenderer;

    [Tooltip("Base sorting order added on top of the Y-sorted value. Useful to layer groups of sprites.")]
    [SerializeField] private int baseSortingOrder = 0;

    [Tooltip("How strongly Y affects sorting. 100 is common for unit/world space in meters.")]
    [Min(1)]
    [SerializeField] private int yToOrderMultiplier = 100;

    [Header("Reference Point (Pivot)")]
    [Tooltip("If enabled, the custom reference point is used for sorting instead of the object's position.")]
    [SerializeField] private bool useCustomPivot = true;

    [Tooltip("Optional transform to use as the reference point (e.g., a child placed at the character's feet). If set, this takes precedence over Local Offset.")]
    [SerializeField] private Transform pivotTransform;

    [Tooltip("Local-space offset from this GameObject's transform used as the reference point when no pivotTransform is provided.")]
    [SerializeField] private Vector2 pivotLocalOffset = Vector2.zero;

    [Header("One-Time Sorting")]
    [Tooltip("If enabled, the sorting order is calculated once on Start (after an optional delay), then this component disables itself.")]
    [SerializeField] private bool calculateOnceOnStart = false;

    [Tooltip("Delay (in seconds) before the initial sorting is applied when 'Calculate Once On Start' is enabled.")]
    [Min(0f)]
    [SerializeField] private float initialSortDelay = 0f;

    [Header("Gizmo")]
    [SerializeField] private bool showPivotGizmo = true;
    [SerializeField] private float gizmoSize = 0.075f;

    // Cache to avoid allocation each frame
    private Vector3 _pivotWorldPos;

    private void Reset()
    {
        targetRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnValidate()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<SpriteRenderer>();
        yToOrderMultiplier = Mathf.Max(1, yToOrderMultiplier);
        gizmoSize = Mathf.Max(0.001f, gizmoSize);

        // Still apply sorting in editor when values change
        ApplySorting();
    }

    private void Start()
    {
        // Only run this one-time logic in play mode
        if (calculateOnceOnStart && Application.isPlaying)
        {
            StartCoroutine(DelayedInitialSort());
        }
    }

    private void LateUpdate()
    {
        // In normal mode, keep updating every frame
        if (!calculateOnceOnStart)
        {
            ApplySorting();
        }
        // If calculateOnceOnStart == true, we let the coroutine handle the one-time sort
    }

    /// <summary>
    /// Coroutine that waits for the configured delay, applies sorting once, then disables this component.
    /// </summary>
    private IEnumerator DelayedInitialSort()
    {
        if (initialSortDelay > 0f)
            yield return new WaitForSeconds(initialSortDelay);

        ApplySorting();

        // Disable this component so it no longer updates
        enabled = false;
    }

    /// <summary>
    /// Computes the world-space reference point used for sorting.
    /// </summary>
    private Vector3 GetReferencePointWorld()
    {
        if (useCustomPivot)
        {
            if (pivotTransform != null)
                return pivotTransform.position;

            // Local offset relative to this transform
            return transform.TransformPoint(new Vector3(pivotLocalOffset.x, pivotLocalOffset.y, 0f));
        }

        // Default: object position
        return transform.position;
    }

    private void ApplySorting()
    {
        if (targetRenderer == null) return;

        _pivotWorldPos = GetReferencePointWorld();

        // Higher Y => smaller (more negative) sorting order
        int yOrder = -Mathf.RoundToInt(_pivotWorldPos.y * yToOrderMultiplier);
        targetRenderer.sortingOrder = baseSortingOrder + yOrder;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showPivotGizmo) return;

        // Draw at the actual reference point used for sorting (handles edit mode too)
        Vector3 p = Application.isPlaying ? _pivotWorldPos : GetReferencePointWorld();

        Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
        Handles.color = new Color(0.2f, 0.9f, 1f, 0.9f);

        // Crosshair
        Vector3 right = Vector3.right * gizmoSize;
        Vector3 up = Vector3.up * gizmoSize;
        Handles.DrawLine(p - right, p + right);
        Handles.DrawLine(p - up, p + up);

        // Circle
        Handles.DrawWireDisc(p, Vector3.forward, gizmoSize);

        // Label (small)
        GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
        style.fontSize = 10;
        style.normal.textColor = Handles.color;
        Handles.Label(p + Vector3.up * (gizmoSize * 1.2f), "Y-Sort Pivot", style);
    }
#endif
}
