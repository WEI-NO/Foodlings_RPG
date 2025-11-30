using CustomLibrary.SpriteExtra;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterDisplay : BaseDraggableCharacterDisplay
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI summonCostText;


    [Header("References")]
    public GameObject equipSign;


    public override bool UpdateInfo()
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

        if (summonCostText) summonCostText.text = $"{assignedInstance.baseData.summonCost}";

        UpdateEquipSign();
        return true;
    }

    public void UpdateEquipSign()
    {
        if (equipSign)
            equipSign.SetActive(assignedInstance != null && assignedInstance.inPartyIndex != -1);
    }

    protected override bool OnReleaseObjects(GameObject obj)
    {
        var pedestal = obj.GetComponentInParent<CharacterPedestal>();
        if (pedestal != null)
        {
            pedestal.AddCharacterToTeam(assignedInstance);
        }

        // Refresh equip sign (in case pedestal changed party index)
        UpdateEquipSign();
        return true;
    }

    protected override void OnClickAction(PointerEventData eventData)
    {
        CharacterUpgradePage.Instance.InitializeCharacter(assignedInstance);
        CharacterUpgradePage.Instance.SetActive(true);
    }

    // Optional: expose ID to drop targets
    public string InstanceId => assignedInstance != null ? assignedInstance.instanceId : null;
}
