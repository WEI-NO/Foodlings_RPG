using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Testing Settings")]
    public List<string> defaultAddItem = new();
    public List<int> defaultAddItemQuantity = new();

    private void Awake()
    {
        PlayerInventory.OnInventoryChange += (i, q) => { print($"Added ({q}): {i.def.Id}"); };
    }
    private void Start()
    {
        int i = 0;
        foreach (var item in defaultAddItem)
        {
            AddItem(item, defaultAddItemQuantity[i]);
            i++;
        }
    }

    public static Action<InventoryItem_I, int> OnInventoryChange;
    public static Dictionary<string, InventoryItem_I> InventoryByID = new();
    public static Dictionary<ItemCategory, Dictionary<string, InventoryItem_I>> InventoryByCategory = new();

    public static int ItemCount(string itemId)
    {
        if (HasItem(itemId))
        {
            return InventoryByID[itemId].quantity;
        }
        return 0;
    }

    public static bool UseItem(string itemId, int quantity)
    {
        if (!HasItem(itemId, quantity)) return false;

        if (InventoryByID.TryGetValue(itemId, out InventoryItem_I item))
        {
            item.quantity -= quantity;
            OnInventoryChange?.Invoke(item, -quantity);
            return true;
        }

        return false;
    }

    public static bool HasItem(string itemID, int quantity = 0)
    {
        if (InventoryByID.TryGetValue(itemID, out InventoryItem_I item))
        {
            return item.quantity >= quantity;
        }
        return false;
    }

    public static bool AddItem(string itemID, int quantity)
    {
        var database = MainDatabase.Instance.itemDatabase;
        if (database == null)
            return false;

        if (ContainsItem(itemID, out var item))
        {
            item.quantity += quantity; // Future: Check for stack
            OnInventoryChange?.Invoke(item, quantity);
        }
        else
        {
            var itemBase = database.Get(itemID);
            if (itemBase != null)
            {
                InventoryItem_I instance = new InventoryItem_I();
                instance.Initialize(itemBase, quantity);
                AddToInventory(instance);
                OnInventoryChange?.Invoke(instance, quantity);
                return true;
            }
        }

        return false;
    }

    private static void AddToInventory(InventoryItem_I item)
    {
        InventoryByID.Add(item.def.Id, item);

        if (!InventoryByCategory.TryGetValue(item.def.category, out var dict))
        {
            dict = new Dictionary<string, InventoryItem_I>();
            InventoryByCategory[item.def.category] = dict;
        }

        dict[item.def.Id] = item;
    }


    public static bool ContainsItem(string id, out InventoryItem_I item)
    {
        if (InventoryByID.ContainsKey(id))
        {
            item = InventoryByID[id];
            return true;
        }

        item = null;
        return false;
    }

    // ItemCategory.Count == ALL
    public static Dictionary<string, InventoryItem_I> GetItems(ItemCategory category = ItemCategory.Count)
    {
        if (category == ItemCategory.Count)
        {
            return InventoryByID;
        }

        if (InventoryByCategory.TryGetValue(category, out var dict))
        {
            return dict;
        }

        return new();
    }
}
