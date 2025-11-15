using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class GhostObject : MonoBehaviour
{
    public static GhostObject Instance { get; private set; }

    [Header("Components (Auto)")]
    [Tooltip("Image used to render the drag ghost.")]
    [SerializeField] private Image ghostImage;
    //[SerializeField] private CanvasGroup ghostCanvasGroup;

    public CharacterInstance assignedInstance;
    private RectTransform rt;
    private Canvas rootCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GhostObject: Multiple instances found; keeping the first.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        rt = GetComponent<RectTransform>();
        if (rt == null)
            rt = gameObject.AddComponent<RectTransform>();

        if (ghostImage == null)
            ghostImage = GetComponent<Image>() ?? gameObject.AddComponent<Image>();

        //if (ghostCanvasGroup == null)
        //    ghostCanvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        ghostImage.raycastTarget = false;

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null || !rootCanvas.isRootCanvas)
            Debug.LogWarning("GhostObject: Please place this under a ROOT Canvas used for dragging.");

        Hide();
    }

    public void Show(CharacterInstance instance, Vector2 size, float alpha = 0.9f)
    {
        if (instance == null) return;

        assignedInstance = instance;
        ghostImage.enabled = true;
        ghostImage.sprite =  assignedInstance.baseData.unitSprite;
        rt.sizeDelta = size;

        ChangeAlpha(alpha);
        //ghostCanvasGroup.alpha = Mathf.Clamp01(alpha);
        //ghostCanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    public void MoveTo(PointerEventData eventData)
    {
        if (!gameObject.activeSelf || rootCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rootCanvas.transform,
            eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out var local
        );
        rt.anchoredPosition = local;
    }

    public void ChangeAlpha(float alpha)
    {
        Color c = ghostImage.color;
        ghostImage.color = new Color(c.r, c.g, c.b, alpha);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        ghostImage.enabled = false;
    }
}
