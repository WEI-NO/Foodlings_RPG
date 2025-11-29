using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterPedestal : BaseDraggableCharacterDisplay
{
    [Header("Components")]
    public CharacterInstance currentData;
    public float iconNormalizedSize = 32;
    public int partyIndex = 0;

    [Header("Unequip (Drag-Off) Settings")]
    [Tooltip("Screen-space distance from drag start after which we consider it an 'unequip' intent.")]
    [SerializeField] private float unequipDistance = 120f;

    [Tooltip("Ghost opacity when cursor is near the start point.")]
    [Range(0f, 1f)][SerializeField] private float nearAlpha = 0.9f;

    [Tooltip("Ghost opacity when cursor is far (>= unequipDistance).")]
    [Range(0f, 1f)][SerializeField] private float farAlpha = 0.5f;

    private Vector2 dragStartScreenPos;
    private float lastDragDistance;
    private bool hadCharacterAtDragStart;

    public void AddCharacterToTeam(CharacterInstance instance)
    {
        PlayerParty.Instance.AddToParty(instance, partyIndex);
        SetCharacter(instance);
    }

    public void SetCharacter(CharacterInstance data)
    {
        currentData = data;
        assignedInstance = data; // keep base field in sync

        if (currentData != null && currentData.baseData != null && currentData.baseData.unitSprite != null)
        {
            Sprite sprite = currentData.baseData.unitSprite;
            characterIcon.gameObject.SetActive(true);
            characterIcon.sprite = sprite;

            float ratio = sprite.rect.width / sprite.rect.height;
            characterIcon.rectTransform.sizeDelta = new Vector2(iconNormalizedSize * ratio, iconNormalizedSize);
            characterIcon.color = normalColor;
        }
        else
        {
            characterIcon.gameObject.SetActive(false);
        }
    }

    protected override void OnBeginDragAction()
    {
        // Remember where the drag started and whether we had a character
        dragStartScreenPos = Input.mousePosition;
        lastDragDistance = 0f;
        hadCharacterAtDragStart = (currentData != null);

        // Ensure initial ghost alpha is the "near" value
        SetGhostAlpha(nearAlpha);
    }

    protected override void OnDraggingAction()
    {
        // Update distance from start and fade ghost accordingly
        lastDragDistance = Vector2.Distance((Vector2)Input.mousePosition, dragStartScreenPos);

        // Simple step fade; replace with a smooth LERP if you prefer
        if (lastDragDistance >= unequipDistance)
            SetGhostAlpha(farAlpha);
        else
            SetGhostAlpha(nearAlpha);
    }

    protected override void OnEndDragAction()
    {
        // If we started with a character and dragged far enough, unequip on release
        if (hadCharacterAtDragStart && lastDragDistance >= unequipDistance)
        {
            // Remove from the party at this index
            PlayerParty.Instance.AddToParty(null, partyIndex);

            //// Clear pedestal visuals/data
            //SetCharacter(null);
        }
    }

    protected override bool OnReleaseObjects(GameObject obj)
    {
        // No special drop-target handling here.
        // If you want pedestal→pedestal swaps or other behavior,
        // you can detect the target components here and act accordingly.
        var pedestal = obj.GetComponentInParent<CharacterPedestal>();

        if (pedestal == null || pedestal == this)
            return false;

        PlayerParty.Instance.AddToParty(assignedInstance, pedestal.partyIndex);
        hadCharacterAtDragStart = false;
        return true;
    }

    // --- helpers ---
    private void SetGhostAlpha(float alpha)
    {
        if (GhostObject.Instance == null) return;
        GhostObject.Instance.ChangeAlpha(alpha);
    }

    protected override void OnClickAction(PointerEventData eventData)
    {
        if (assignedInstance == null || assignedInstance.baseData == null) return;
        CharacterUpgradePage.Instance.InitializeCharacter(assignedInstance);
        CharacterUpgradePage.Instance.SetActive(true);
    }
}
