using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI_SortButton : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI categoryText;
    public Image border;

    public float selectedAlpha;
    public float deselectAlpha;

    public ItemCategory category;

    public void Initialize(ItemCategory category)
    {
        this.category = category;
        categoryText.text = category == ItemCategory.Count ? "All" : category.ToString();
    }    

    public void SetSelected(bool state)
    {
        if (state)
        {
            var color = border.color;
            color.a = selectedAlpha;
            border.color = color;
        } else
        {
            var color = border.color;
            color.a = deselectAlpha;
            border.color = color;
        }
    }

    public void OnClick()
    {
        InventoryUI_CategoryPanel.Instance.SelectCategory(category);
    }
}
