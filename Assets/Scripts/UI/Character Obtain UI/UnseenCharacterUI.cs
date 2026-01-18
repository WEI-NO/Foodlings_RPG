using CustomLibrary.References;
using CustomLibrary.SpriteExtra;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnseenCharacterUI : MonoBehaviour
{
    private Animator anim;

    [Header("UI References")]
    public Image characterIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI continueHintText;
    public CharacterRankDisplay rankDisplay;

    public KeyCode skipKeyCode;
    public bool InProgress = false;
    public float animationDurationDelay = 3.5f;
    public float animationEndingDelay = 2.0f;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (InProgress && Input.GetKeyDown(skipKeyCode))
        {
            EndView();
        }
    }

    public void Init(CharacterData data)
    {
        InProgress = false;
        if (data == null) return;

        characterIcon.sprite = data.unitSprite;
        SpriteExtra.SetDynamicDimension(characterIcon, 100.0f);
        nameText.text = $"{data.displayName}";
        roleText.text = $"{data.role.ToString()}";
        descriptionText.text = $"";
        rankDisplay.Init(data.baseRank.ToInt());
    }

    public IEnumerator StartView()
    {
        anim.SetTrigger("Start");
        yield return new WaitForSeconds(animationDurationDelay);
        InProgress = true;
        while (InProgress)
        {
            yield return new WaitForSeconds(animationEndingDelay);
            yield return null;
        }
    }

    public void EndView()
    {
        InProgress = false;
        anim.SetTrigger("End");
    }

}
