using CustomLibrary.References;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UP_InventoryCollectionDisplay : MonoBehaviour
{
    public static UP_InventoryCollectionDisplay Instance;

    public Transform content;
    public InventoryUI_Display displayPrefab;
    public Dictionary<string, InventoryUI_Display> displays = new();

    public InventoryUI_ItemInfo infoPanel;

    private void Awake()
    {
        Initializer.SetInstance(this);
        PlayerInventory.OnInventoryChange += OnItemChange;
    }

    private void OnEnable()
    {
        RefreshDisplay();

    }

    private void Start()
    {

    }

    public void OnItemChange(InventoryItem_I instance, int newQuantity)
    {
        if (displays.TryGetValue(instance.def.Id, out var display))
        {
            if (display == null) return;
            display.UpdateDisplay();
        }
    }

    private void RefreshDisplay()
    {
        ClearDisplay();

        AddAllItems();
    }
    
    public void ShowItemsByCategory(ItemCategory category)
    {
        ClearDisplay();

        var inventory = PlayerInventory.GetItems(category);

        foreach (var pair in inventory)
        {
            if (pair.Value != null)
            {
                var newDisplay = Instantiate(displayPrefab, content);
                newDisplay.Initialize(pair.Value);
                displays.Add(pair.Key, newDisplay);
            }
        }
    }

    private void AddAllItems()
    {
        foreach (var pair in PlayerInventory.InventoryByID)
        {
            if (pair.Value != null)
            {
                var newDisplay = Instantiate(displayPrefab, content);
                newDisplay.Initialize(pair.Value);
                displays.Add(pair.Key, newDisplay);
            }
        }
    }

    private void ClearDisplay()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        displays.Clear();
    }
}
