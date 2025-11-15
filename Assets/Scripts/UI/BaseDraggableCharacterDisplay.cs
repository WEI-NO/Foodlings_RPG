using CustomLibrary.SpriteExtra;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BaseDraggableCharacterDisplay : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    #region Base Class
    protected virtual void OnAwake() { }
    protected virtual void OnEnabled() { }
    protected virtual void OnStart() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnDisabled() { }
    protected virtual void OnDestroyed() { }

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        OnAwake();
    }
    private void OnEnable() { OnEnabled(); }
    private void Start() { OnStart(); }
    private void Update() { OnUpdate(); }
    private void FixedUpdate() { OnFixedUpdate(); }
    private void OnDisable() { OnDisabled(); }
    private void OnDestroy() { OnDestroyed(); }
    #endregion base class

    [Header("Character")]
    public CharacterInstance assignedInstance;
    public float iconDisplaySize = 96f;
    public float draggingDisplaySize = 128f;
    public Color draggingColor = Color.white;
    public Color normalColor = Color.white;

    [Header("Visual Settings")]
    public Image characterIcon;

    [Header("Dragging")]
    private CanvasGroup cg;
    private bool draggingActive = false;

    // --- Hook points ---
    protected virtual void OnBeginDragAction() { }
    protected virtual void OnDraggingAction() { }
    protected virtual void OnEndDragAction() { }

    // NEW: fires only on click/tap (no drag)
    protected virtual void OnClickAction(PointerEventData eventData) { }

    /// <summary>
    /// Return true if the release was handled by a drop target.
    /// </summary>
    protected virtual bool OnReleaseObjects(GameObject obj) { return true; }

    public virtual bool UpdateInfo() { return true; }

    public void AssignInstance(CharacterInstance instance)
    {
        assignedInstance = instance;
        assignedInstance.OnLevelUp += (i, j) => { UpdateInfo(); };
        UpdateInfo();
    }

    // ===== Drag & Drop =====
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedInstance == null || assignedInstance.baseData == null || assignedInstance.baseData.unitSprite == null)
            return;

        draggingActive = true;

        // Let raycasts pass through this card while dragging so drop targets receive events
        if (cg) cg.blocksRaycasts = false;

        // Visual hint on the original icon
        if (characterIcon) characterIcon.color = draggingColor;

        // Spawn/Show ghost from shared GhostObject
        if (GhostObject.Instance != null)
        {
            var dim = SpriteExtra.DynamicDimension(assignedInstance.baseData.unitSprite, draggingDisplaySize);
            GhostObject.Instance.Show(assignedInstance, dim, 0.9f);
            GhostObject.Instance.MoveTo(eventData);
        }
        else
        {
            Debug.LogWarning("CharacterDisplayDragHandler: No GhostObject in scene. Drag ghost will be disabled.");
        }

        OnBeginDragAction();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GhostObject.Instance != null)
            GhostObject.Instance.MoveTo(eventData);

        OnDraggingAction();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore raycasts on original card
        if (cg) cg.blocksRaycasts = true;

        // Drop: detect targets under cursor
        if (EventSystem.current != null && assignedInstance != null)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (!go) continue;

                if (OnReleaseObjects(go)) break;
            }
        }

        // Hide the shared ghost
        if (GhostObject.Instance != null)
            GhostObject.Instance.Hide();

        // Restore visual color
        if (characterIcon) characterIcon.color = normalColor;

        OnEndDragAction();

        draggingActive = false;
    }

    // ===== Click (no drag) =====
    public void OnPointerClick(PointerEventData eventData)
    {
        // Unity won't send OnPointerClick after a drag, but we also guard explicitly.
        if (!draggingActive)
        {
            OnClickAction(eventData);
        }
    }
}
