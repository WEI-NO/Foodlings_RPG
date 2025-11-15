using CustomLibrary.References;
using UnityEngine;

public class InventoryUI_CategoryPanel : MonoBehaviour
{
    public static InventoryUI_CategoryPanel Instance;

    [Header("References")]
    public InventoryUI_SortButton sortButtonPrefab;
    public Transform content;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void OnEnable()
    {
        RefreshPanel();
    }

    private void RefreshPanel()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        var allButton = Instantiate(sortButtonPrefab, content);
        allButton.Initialize(ItemCategory.Count);

        for (int i = 0; i < ItemCategory.Count.ToInt(); i++)
        {
            var newButton = Instantiate(sortButtonPrefab, content);
            newButton.Initialize((ItemCategory)i);
        }

        SelectCategory(ItemCategory.Count);
    }

    public void SelectCategory(ItemCategory category)
    {
        int index = category.ToInt() + 1;

        if (category == ItemCategory.Count)
        {
            content.GetChild(0).GetComponent<InventoryUI_SortButton>().SetSelected(true);
        } else
        {
            content.GetChild(0).GetComponent<InventoryUI_SortButton>().SetSelected(false);
        }
        for (int i = 1; i < content.childCount; i++)
        {
            var button = content.GetChild(i).GetComponent<InventoryUI_SortButton>();
            if (index == i) button.SetSelected(true);
            else button.SetSelected(false);
        }

        UP_InventoryCollectionDisplay.Instance?.ShowItemsByCategory(category);
    }

}
