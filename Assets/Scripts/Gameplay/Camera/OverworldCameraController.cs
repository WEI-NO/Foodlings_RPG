using CustomLibrary.References;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems; // <-- needed for UI hit checks

[RequireComponent(typeof(Camera))]
public class OverworldCameraController : MonoBehaviour
{
    public static OverworldCameraController Instance;

    [Header("Movement Settings")]
    [Tooltip("Base movement speed (WASD or mouse edge).")]
    public float moveSpeed = 10f;

    [Header("Mouse Edge Settings")]
    [Tooltip("Pixels from the screen edge that trigger edge-panning.")]
    public float edgeThreshold = 25f;
    public bool enableMouseEdgeMovement = true;

    [Header("Zoom Settings (Orthographic)")]
    public float zoomSpeed = 5f;
    public float minZoom = 5f;
    public float maxZoom = 20f;

    [Header("World Bounds (Auto from Tilemap)")]
    [Tooltip("Tilemap that defines the playable/visible world. Its painted tiles define the bounds.")]
    public Tilemap overworldBase;

    // Internally computed world-space 2D bounds (min/max X,Y)
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Camera cam;

    [Header("Control Gate")]
    [Tooltip("When false, player input (WASD/edge/zoom) is ignored. Auto-pan can still move the camera.")]
    public bool Controllable = true;

    [Header("Input & UI")]
    [Tooltip("If true, any pointer over UI will block WASD, drag/edge pan, and scroll zoom.")]
    public bool blockInputWhenPointerOverUI = true;

    [Header("Auto Pan Queue")]
    [Tooltip("Default travel time (seconds) if not supplied in EnqueuePan overload.")]
    public float defaultTravelTime = 1.0f;
    [Tooltip("Default dwell time (seconds) if not supplied in EnqueuePan overload.")]
    public float defaultDwellTime = 0.0f;
    [Tooltip("Optional easing curve for travel (0→1). Leave null for linear.")]
    public AnimationCurve travelCurve = null;

    private struct PanWaypoint
    {
        public Vector2 pos;
        public float travelTime;
        public float dwellTime;
        public float targetZoom;

        public PanWaypoint(Vector2 p, float t, float d, float z)
        {
            pos = p;
            travelTime = Mathf.Max(0f, t);
            dwellTime = Mathf.Max(0f, d);
            targetZoom = z;
        }
    }

    private readonly Queue<PanWaypoint> panQueue = new Queue<PanWaypoint>();
    private Coroutine panRoutine;

    public bool IsAutoPanning => panRoutine != null;

    void Awake()
    {
        Initializer.SetInstance(this);
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void Start()
    {
        transform.position = ClampToBounds(transform.position, cam.orthographicSize);
    }

    void OnEnable()
    {
        if (Application.isPlaying)
            RefreshBoundsFromTilemap();
    }

    void Update()
    {
        HandleMovement(); // early-outs if UI hovered or not Controllable
        HandleZoom();     // early-outs if UI hovered or not Controllable
    }

    // ===== UI-block helper =====
    private bool IsPointerOverUI()
    {
        if (!blockInputWhenPointerOverUI) return false;
        if (EventSystem.current == null) return false;

        // Mouse / standalone
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        // Touch devices: check every finger id
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                return true;
        }
        return false;
    }

    // --- Core: read world bounds from the tilemap's painted area
    private void UpdateBoundsFromTilemap()
    {
        if (!overworldBase) return;

        overworldBase.RefreshAllTiles();
        overworldBase.CompressBounds();

        BoundsInt cb = overworldBase.cellBounds;
        if (cb.size.x == 0 || cb.size.y == 0) return;

        Vector3 worldMin = overworldBase.CellToWorld(new Vector3Int(cb.xMin, cb.yMin, 0));
        Vector3 worldMax = overworldBase.CellToWorld(new Vector3Int(cb.xMax, cb.yMax, 0));

        minBounds = new Vector2(Mathf.Min(worldMin.x, worldMax.x), Mathf.Min(worldMin.y, worldMax.y));
        maxBounds = new Vector2(Mathf.Max(worldMin.x, worldMax.x), Mathf.Max(worldMin.y, worldMax.y));

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, GetMaxAllowedZoom());
        transform.position = ClampToBounds(transform.position, cam.orthographicSize);
    }

    private void HandleMovement()
    {
        if (!Controllable) return;

        Vector3 moveDir = Vector3.zero;

        // Keyboard
        if (Input.GetKey(KeyCode.W)) moveDir.y += 1f;
        if (Input.GetKey(KeyCode.S)) moveDir.y -= 1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1f;
        
        if (!IsPointerOverUI())
        {
            // Mouse edge pan
            if (enableMouseEdgeMovement)
            {
                Vector3 m = Input.mousePosition;
                if (m.y >= Screen.height - edgeThreshold) moveDir.y += 1f;
                if (m.y <= edgeThreshold) moveDir.y -= 1f;
                if (m.x >= Screen.width - edgeThreshold) moveDir.x += 1f;
                if (m.x <= edgeThreshold) moveDir.x -= 1f;
            }
        }


        if (moveDir != Vector3.zero)
        {
            moveDir.Normalize();

            float adjustedSpeed = moveSpeed * (cam.orthographicSize / Mathf.Max(minZoom, 0.0001f));

            Vector3 target = transform.position + moveDir * adjustedSpeed * Time.deltaTime;
            transform.position = ClampToBounds(target, cam.orthographicSize);
        }
    }

    private void HandleZoom()
    {
        if (!Controllable) return;
        if (IsPointerOverUI()) return; // <-- block scrolling when pointer is over UI

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float desired = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            float allowed = GetMaxAllowedZoom();
            cam.orthographicSize = Mathf.Clamp(desired, minZoom, allowed);

            transform.position = ClampToBounds(transform.position, cam.orthographicSize);
        }
    }

    // Returns the largest orthographicSize that still keeps camera edges within the tilemap bounds.
    private float GetMaxAllowedZoom()
    {
        if (maxBounds == minBounds) return maxZoom;

        float worldWidth = maxBounds.x - minBounds.x;
        float worldHeight = maxBounds.y - minBounds.y;

        float byHeight = worldHeight * 0.5f;
        float byWidth = (worldWidth * 0.5f) / Mathf.Max(cam.aspect, 0.0001f);

        float geometricMax = Mathf.Max(0.0001f, Mathf.Min(byHeight, byWidth));
        return Mathf.Min(maxZoom, geometricMax);
    }

    // Clamp camera center so that the VIEWPORT edges stay within bounds for the given ortho size.
    private Vector3 ClampToBounds(Vector3 targetPos, float orthoSize)
    {
        float halfH = orthoSize;
        float halfW = orthoSize * cam.aspect;

        float minX = minBounds.x + halfW;
        float maxX = maxBounds.x - halfW;
        float minY = minBounds.y + halfH;
        float maxY = maxBounds.y - halfH;

        if (minX > maxX) targetPos.x = (minBounds.x + maxBounds.x) * 0.5f;
        else targetPos.x = Mathf.Clamp(targetPos.x, minX, maxX);

        if (minY > maxY) targetPos.y = (minBounds.y + maxBounds.y) * 0.5f;
        else targetPos.y = Mathf.Clamp(targetPos.y, minY, maxY);

        return new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    // Call this if you repaint tiles at runtime and need bounds to refresh
    public void RefreshBoundsFromTilemap() => UpdateBoundsFromTilemap();

    /// <summary> Enqueue a world-position to pan to with explicit travel/dwell times and zoom target. </summary>
    public void EnqueuePan(Vector2 worldPos, float travelTime, float dwellTime, float targetZoom,
                           bool clearExisting = false, bool lockControlsDuringPan = true)
    {
        if (clearExisting)
        {
            ClearPanQueue();
            StopAutoPan(false);
        }

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        panQueue.Enqueue(new PanWaypoint(worldPos, travelTime, dwellTime, targetZoom));

        if (panRoutine == null)
            panRoutine = StartCoroutine(ProcessPanQueue(lockControlsDuringPan));
    }

    /// <summary> Overload: same as before (keeps current zoom). </summary>
    public void EnqueuePan(Vector2 worldPos, float travelTime, float dwellTime,
                           bool clearExisting = false, bool lockControlsDuringPan = true)
    {
        EnqueuePan(worldPos, travelTime, dwellTime, cam.orthographicSize, clearExisting, lockControlsDuringPan);
    }

    /// <summary> Back-compat overload: uses defaults (and keeps current zoom). </summary>
    public void EnqueuePan(Vector2 worldPos, bool clearExisting = false, bool lockControlsDuringPan = true)
    {
        EnqueuePan(worldPos, defaultTravelTime, defaultDwellTime, cam.orthographicSize, clearExisting, lockControlsDuringPan);
    }

    public void ClearPanQueue() => panQueue.Clear();

    public void StopAutoPan(bool restoreControls = true)
    {
        if (panRoutine != null)
        {
            StopCoroutine(panRoutine);
            panRoutine = null;
        }
        if (restoreControls) Controllable = true;
    }

    private IEnumerator ProcessPanQueue(bool lockControlsDuringPan)
    {
        MainHUDController.Instance.SetHidden(true);

        bool prevControllable = Controllable;
        if (lockControlsDuringPan) Controllable = false;

        while (panQueue.Count > 0)
        {
            PanWaypoint next = panQueue.Dequeue();
            yield return SmoothPanTo(next.pos, next.travelTime, next.dwellTime, next.targetZoom);
        }

        if (lockControlsDuringPan) Controllable = prevControllable;
        panRoutine = null;
        MainHUDController.Instance.SetHidden(false);
    }

    /// <summary> Smoothly pans to a world position while adjusting zoom. </summary>
    private IEnumerator SmoothPanTo(Vector2 targetWorldPos, float travelTime, float dwellTime, float targetZoom)
    {
        Vector3 clampedTarget = ClampToBounds(
            new Vector3(targetWorldPos.x, targetWorldPos.y, transform.position.z),
            cam.orthographicSize);

        float startZoom = cam.orthographicSize;
        float clampedTargetZoom = Mathf.Clamp(targetZoom, minZoom, GetMaxAllowedZoom());

        if (travelTime <= 0.0001f)
        {
            transform.position = clampedTarget;
            cam.orthographicSize = clampedTargetZoom;
        }
        else
        {
            Vector3 start = transform.position;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, travelTime);
                float k = Mathf.Clamp01(t);

                float easedT = (travelCurve != null && travelCurve.length > 0)
                    ? Mathf.Clamp01(travelCurve.Evaluate(k))
                    : Mathf.SmoothStep(0f, 1f, k);

                Vector3 pos = Vector3.Lerp(start, clampedTarget, easedT);
                pos = ClampToBounds(pos, cam.orthographicSize);
                cam.orthographicSize = Mathf.Lerp(startZoom, clampedTargetZoom, easedT);
                transform.position = pos;

                yield return null;
            }

            transform.position = clampedTarget;
            cam.orthographicSize = clampedTargetZoom;
        }

        if (dwellTime > 0f)
            yield return new WaitForSeconds(dwellTime);
    }
}
