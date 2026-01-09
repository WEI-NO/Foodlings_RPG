using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Inventory Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("All item definitions in game")]
    public List<InventoryItem> items = new List<InventoryItem>();

#if UNITY_EDITOR
    [ContextMenu("Rebuild Database")]
    public void Rebuild()
    {
        items.Clear();

        string[] guids = AssetDatabase.FindAssets("t:InventoryItem");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            InventoryItem item = AssetDatabase.LoadAssetAtPath<InventoryItem>(path);
            if (item != null)
                items.Add(item);
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"ItemDatabase rebuilt. {items.Count} items found.");
    }
#endif

    // Runtime lookup table
    private Dictionary<string, InventoryItem> lookup;

    /// <summary>
    /// Returns the item definition by ID
    /// </summary>
    public InventoryItem Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        EnsureLookup();
        lookup.TryGetValue(id, out var def);
        return def;
    }

    /// <summary>
    /// Returns all definitions
    /// </summary>
    public List<InventoryItem> GetAll()
    {
        EnsureLookup();
        return items;
    }

    /// <summary>
    /// Check if database contains this item
    /// </summary>
    public bool Contains(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureLookup();
        return lookup.ContainsKey(id);
    }

    /// <summary>
    /// Build lookup if needed
    /// </summary>
    private void EnsureLookup()
    {
        if (lookup != null) return;

        lookup = new Dictionary<string, InventoryItem>();

        foreach (var item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventoryDatabase] Null item in list.");
                continue;
            }

            // Ensure unique ID
            if (string.IsNullOrEmpty(item.Id))
            {
                Debug.LogWarning($"[InventoryDatabase] Item missing ID: {item.name}");
                continue;
            }

            if (lookup.ContainsKey(item.Id))
            {
                Debug.LogWarning($"[InventoryDatabase] Duplicate Item ID found: {item.Id}");
                continue;
            }

            lookup.Add(item.Id, item);
        }
    }

#if UNITY_EDITOR
    // Automatically rebuild lookup when changed in Editor
    private void OnValidate()
    {
        lookup = null;
        EnsureLookup();
    }
#endif
}
