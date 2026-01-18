using CustomLibrary.References;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewObtainedCharacterDisplay : MonoBehaviour
{
    private Animator anim;

    [Header("UI References")]
    public CharacterRankDisplay rankDisplay;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI costText;
    public Image characterIcon;
    public Image roleIcon;
    public GameObject unseenIndicator;

    [Header("References")]
    public CharacterData characterData;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void Init(CharacterData data, bool seen)
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
        unseenIndicator.SetActive(!seen);
    }

    public void RunAnimation_Default()
    {
        if (anim == null) return;
        anim.SetTrigger("DefaultReveal");
    }

    public void RunAnimation_Seen()
    {
        if (anim == null) return;
        anim.SetTrigger("NewReveal");
    }

    public void RunAnimation_Continuation()
    {
        if (anim == null) return;
        anim.SetTrigger("Continuation");
    }

}
