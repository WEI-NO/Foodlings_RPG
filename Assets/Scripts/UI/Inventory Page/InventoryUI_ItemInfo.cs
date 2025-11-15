using CustomLibrary.References;
using CustomLibrary.SpriteExtra;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI_ItemInfo : MonoBehaviour
{
    public static InventoryUI_ItemInfo Instance;

    [Header("Components")]
    public Image icon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI nameText;

    public InventoryItem_I currentItem;
    public float iconSize = 32;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void OnEnable()
    {
        ShowItem(null);
    }

    public void ShowItem(InventoryItem_I item)
    {
        currentItem = item;

        if (item == null)
        {
            icon.gameObject.SetActive(false);
            quantityText.text = "???";
            descriptionText.text = "???";
            nameText.text = "???";
        }
        else
        {
            icon.gameObject.SetActive(true);
            icon.sprite = item.def.Icon;
            var size = SpriteExtra.DynamicDimension(icon.sprite, iconSize);
            icon.rectTransform.sizeDelta = size;
            quantityText.text = $"{item.quantity}";
            descriptionText.text = $"{item.def.description}";
            nameText.text = item.def.DisplayName;
        }
    }
}
