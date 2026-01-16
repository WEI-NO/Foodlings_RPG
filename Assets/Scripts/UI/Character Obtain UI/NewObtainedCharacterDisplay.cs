using CustomLibrary.References;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewObtainedCharacterDisplay : MonoBehaviour
{
    [Header("UI References")]
    public CharacterRankDisplay rankDisplay;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Image characterIcon;
    public Image roleIcon;
    public GameObject unseenIndicator;

    [Header("References")]
    public CharacterData characterData;

    public void Init(CharacterData data)
    {
        if (data == null) return;

        // Rank Display
        rankDisplay.Init(data.baseRank.ToInt());

        // Basic Info
        nameText.text = $"{data.displayName}";
        costText.text = $"{data.summonCost}";
        characterIcon.sprite = data.unitSprite;
        //characterIcon.SetNativeSize();
        roleIcon.sprite = MainDatabase.Instance.spriteDatabase.Get(SpriteDatabase.GetRoleID(data.role)).sprite;
        unseenIndicator.SetActive(!PlayerCollection.Instance.SeenInCatalog(data.id));
    }

}
