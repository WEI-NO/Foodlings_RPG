using CustomLibrary.SpriteExtra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FuzeCharacterDisplay : MonoBehaviour
{
    [Header("Character Settings")]
    public CharacterInstance heldCharacter;

    [Header("UI Settings")]
    public Image characterIcon;
    public TextMeshProUGUI levelText;
    public float iconSize;
    public GameObject selectedIndicator;
    public bool selected = false;

    public void ToggleSelection()
    {
        var result = CharacterUpgradePage.Instance.ToggleFusion(heldCharacter);
        SetSelected(result);
    }

    public void Initialize(CharacterInstance instance, bool selected = false)
    {
        if (instance == null)
        {
            Destroy(gameObject);
            return;
        }

        heldCharacter = instance;
        var size = SpriteExtra.DynamicDimension(instance.baseData.unitSprite, iconSize);
        characterIcon.sprite = instance.baseData.unitSprite;
        characterIcon.rectTransform.sizeDelta = size;
        levelText.text = instance.level.ToString();
        SetSelected(selected);
    }

    public void SetSelected(bool state)
    {
        if (selectedIndicator == null) return;
        selectedIndicator.SetActive(state);
        this.selected = state;
    }
}
