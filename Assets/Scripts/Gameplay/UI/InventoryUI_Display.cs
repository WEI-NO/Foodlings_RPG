using CustomLibrary.SpriteExtra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI_Display : MonoBehaviour
{
    [Header("Item Display")]
    public InventoryItem_I itemInstance;

    [Header("Components")]
    public Image icon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI rarityText;

    public float iconSize = 32;

    public void Initialize(InventoryItem_I itemInstance)
    {
        this.itemInstance = itemInstance;

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (itemInstance == null)
        {
            icon.gameObject.SetActive(false);
            quantityText.gameObject.SetActive(false);
            rarityText.gameObject.SetActive(false);
        } else
        {
            icon.gameObject.SetActive(true);
            quantityText.gameObject.SetActive(true);
            rarityText.gameObject.SetActive(true);

            icon.sprite = itemInstance.def.Icon;
            var size = SpriteExtra.DynamicDimension(icon.sprite, iconSize);
            icon.rectTransform.sizeDelta = size;
            quantityText.text = $"x{itemInstance.quantity}";
        }
    }
    
    public void OnClick()
    {
        InventoryUI_ItemInfo.Instance.ShowItem(itemInstance);
    }

}
