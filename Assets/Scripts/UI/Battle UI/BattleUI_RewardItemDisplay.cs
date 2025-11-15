using CustomLibrary.SpriteExtra;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI_RewardItemDisplay : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI quantityText;
    public Image icon;

    [Header("Settings")]
    public float iconSize = 32;
    public InventoryItem item;
    public int quantity;

    public void ShowReward(InventoryItem item, int quantity)
    {
        this.quantity = quantity;
        this.item = item;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (item == null)
        {
            Destroy(gameObject);
        }
        else
        {
            icon.sprite = item.Icon;
            var size = SpriteExtra.DynamicDimension(icon.sprite, iconSize);
            icon.rectTransform.sizeDelta = size;

            quantityText.text = $"{quantity}";
        }
    }

}
