using CustomLibrary.SpriteExtra;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DraggableDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Visual Settings")]
    public Image characterIcon;
    public float iconDisplaySize = 96f;
    public float draggingDisplaySize = 128f;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public Color draggingColor = Color.white;
    public Color normalColor = Color.white;

    [Header("References")]
    public CharacterInstance assignedInstance;
    public GameObject equipSign;

    [Header("Dragging")]
    [Tooltip("Optional. If null, will auto-find a root Canvas for ray conversions. GhostObject should live under a root Canvas.")]
    public Canvas dragRootCanvas;

    private CanvasGroup cg;

    private void Awake()
    {
        cg = GetComponent<CanvasGroup>();
    }

    public void AssignInstance(CharacterInstance instance)
    {
        assignedInstance = instance;
        UpdateInfo();
    }

    public bool UpdateInfo()
    {
        if (assignedInstance == null || assignedInstance.baseData == null)
        {
            if (characterIcon) characterIcon.gameObject.SetActive(false);
            return false;
        }

        if (characterIcon)
        {
            characterIcon.gameObject.SetActive(true);
            characterIcon.sprite = assignedInstance.baseData.unitSprite;

            var dimension = SpriteExtra.DynamicDimension(assignedInstance.baseData.unitSprite, iconDisplaySize);
            characterIcon.rectTransform.sizeDelta = dimension;
            characterIcon.color = normalColor;
        }

        if (levelText) levelText.text = $"Lv. {assignedInstance.level}";
        if (nameText) nameText.text = assignedInstance.baseData.displayName;

        UpdateEquipSign();
        return true;
    }

    public void UpdateEquipSign()
    {
        if (equipSign)
            equipSign.SetActive(assignedInstance != null && assignedInstance.inPartyIndex != -1);
    }

    // ===== Drag & Drop =====
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (assignedInstance == null || assignedInstance.baseData == null || assignedInstance.baseData.unitSprite == null)
            return;

        // Let raycasts pass through this card while dragging so drop targets receive events
        cg.blocksRaycasts = false;

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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GhostObject.Instance != null)
            GhostObject.Instance.MoveTo(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore raycasts on original card
        cg.blocksRaycasts = true;

        // Drop: detect CharacterPedestal under cursor and add to team
        if (EventSystem.current != null && assignedInstance != null)
        {
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (!go) continue;

                var pedestal = go.GetComponentInParent<CharacterPedestal>();
                if (pedestal != null)
                {
                    // Call your API
                    pedestal.AddCharacterToTeam(assignedInstance);
                    break;
                }
            }
        }

        // Hide the shared ghost
        if (GhostObject.Instance != null)
            GhostObject.Instance.Hide();

        // Restore visual color
        if (characterIcon) characterIcon.color = normalColor;

        // Refresh equip sign (in case pedestal changed party index)
        UpdateEquipSign();
    }

    // Optional: expose ID to drop targets
    public string InstanceId => assignedInstance != null ? assignedInstance.instanceId : null;
}
