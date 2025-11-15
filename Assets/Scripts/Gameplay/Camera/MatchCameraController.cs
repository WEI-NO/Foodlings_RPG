using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class MatchCameraController : MonoBehaviour
{
    [Header("Camera Pan Settings")]
    [SerializeField] private float panSpeed = 5f;        // Speed of camera movement
    [SerializeField] private float edgeThreshold = 100f; // Distance from screen edge (in pixels)
    [SerializeField] private float cameraYAnchor = 0;

    [Header("Clamp Settings")]
    public bool automaticAdjustMinMaxX;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;       // Scroll wheel sensitivity
    [SerializeField] private float minZoom = 3f;         // Minimum orthographic size
    [SerializeField] private float maxZoom = 10f;        // Maximum orthographic size

    private bool ready = false;

    private Camera cam;

    async void Start()
    {
        ready = false;

        while (SceneTransitor.Instance.isTransitioning || LoadingScreen.Instance.IsLoading())
        {
            await Task.Yield();
        }
        
        StartCoroutine(MoveFromRightToLeft(3f)); // pans over 3 seconds

        cam = Camera.main;

        if (automaticAdjustMinMaxX)
        {
            var m = MapController.Instance;
            if (m)
            {
                minX = m.GetBound_Left;
                maxX = m.GetBound_Right;
            }
        }
        ready = true;
    }

    void Update()
    {
        if (!ready) return;
        HandlePan();
        HandleZoom();
    }

    private void HandlePan()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 cameraPos = transform.position;
        float moveDir = 0f;

        // Move left if near the left edge
        if (mousePos.x <= edgeThreshold)
        {
            float factor = 1f - (mousePos.x / edgeThreshold);
            moveDir = -factor;
        }
        // Move right if near the right edge
        else if (mousePos.x >= Screen.width - edgeThreshold)
        {
            float factor = 1f - ((Screen.width - mousePos.x) / edgeThreshold);
            moveDir = factor;
        }

        // Apply movement
        cameraPos.x += moveDir * panSpeed * Time.deltaTime;

        // Clamp camera within bounds
        cameraPos.x = Mathf.Clamp(cameraPos.x, minX, maxX);
        cameraPos.y = cameraYAnchor;
        transform.position = cameraPos;
    }

    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.001f)
        {
            // Adjust zoom (orthographic size)
            cam.orthographicSize -= scrollInput * zoomSpeed;

            // Clamp zoom level
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
        }
    }

    public IEnumerator MoveFromRightToLeft(float duration = 2f)
    {
        while (GameMatchManager.Instance == null || LoadingScreen.Instance.IsLoading())
        {
            yield return null;
        }
        GameMatchManager.Instance.SetGameState(false);
        // Step 1: Snap to rightmost bound
        Vector3 startPos = new Vector3(maxX, cameraYAnchor, transform.position.z);
        transform.position = startPos;

        // Step 2: Smoothly pan to leftmost bound
        Vector3 targetPos = new Vector3(minX, cameraYAnchor, transform.position.z);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        // Step 3: Snap cleanly to final position
        transform.position = targetPos;
        GameMatchManager.Instance.SetGameState(true);
    }


}
