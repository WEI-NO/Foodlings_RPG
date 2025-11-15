
using UnityEngine;

public enum ItemCategory
{
    Currency,
    Equipment,
    Count
}

// ItemDefinition.asset — shared, immutable-ish data
[CreateAssetMenu(menuName = "Items/Item Definition")]
public class InventoryItem : ScriptableObject
{
    [SerializeField] private string id;          // stable GUID/string
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField] private int maxStack = 99999;
    [TextArea] public string description;
    public ItemCategory category;

    public bool noStackLimit = false;
    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public int MaxStack => noStackLimit ? int.MaxValue : maxStack;
}

// Per-copy, mutable runtime selected
[System.Serializable]
public class InventoryItem_I
{
    public InventoryItem def;             // reference to template
    public int quantity = 1;
    // add rolls, enchantments, etc.

    public bool IsStackFull => quantity >= (def ? def.MaxStack : 1);

    public void Initialize(InventoryItem baseItem, int quantity = 0)
    {
        def = baseItem;
        this.quantity = quantity;
    }
}
