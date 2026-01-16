using CustomLibrary.References;
using CustomLibrary.SpriteExtra;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterDisplay : BaseDraggableCharacterDisplay
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI summonCostText;


    [Header("References")]
    public GameObject equipSign;

    public GameObject starObject;
    public Transform starContent;
    public TextMeshProUGUI rarityText;

    public Image roleIcon;
    public float iconRatio = 16.0f;

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

        if (levelText) levelText.text = $"{assignedInstance.level}";
        if (nameText) nameText.text = assignedInstance.baseData.displayName;

        if (summonCostText) summonCostText.text = $"{assignedInstance.baseData.summonCost}";

        if (starContent && starObject)
        {
            int starCount = assignedInstance.rank.ToInt() + 1;

            foreach (Transform child in starContent)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < starCount; i++)
            {
                var star = Instantiate(starObject, starContent);
            }
        }

        if (rarityText)
        {
            rarityText.text = $"{assignedInstance.rank}";
        }

        if (roleIcon)
        {
            Role role = assignedInstance.baseData.role;
            string iconID =
                role == Role.Fighter ? "fighter_icon" :
                role == Role.Tank ? "tank_icon" :
                role == Role.Magic ? "magic_icon" :
                role == Role.Support ? "support_icon" : "fighter_icon";
            Sprite icon = MainDatabase.Instance.spriteDatabase.Get(iconID).sprite;
            var deltaSize = SpriteExtra.DynamicDimension(icon, iconRatio);
            roleIcon.rectTransform.sizeDelta = deltaSize;
            roleIcon.sprite = icon;
        }

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
