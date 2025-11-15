using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ItemDisplay : MonoBehaviour
{
    [Header("Binding")]
    [SerializeField] private string itemId;                 // The ID of the item to display
    [SerializeField] private TextMeshProUGUI countText;     // Label to show the count

    [Header("Options")]
    [SerializeField] private bool hideWhenZero = false;     // Optionally hide when count == 0

    private void OnEnable()
    {
        TrySubscribe();
        RefreshCount();
    }

    private void OnDisable()
    {
        TryUnsubscribe();
    }

    private void Start()
    {
        // In case inventory initializes after Awake/OnEnable in your boot order
        TrySubscribe();
        RefreshCount();
    }

    private void OnValidate()
    {
        // Auto-find label if not wired yet
        if (countText == null) countText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    /// <summary>Assign a new item id at runtime and refresh UI.</summary>
    public void Bind(string newItemId)
    {
        itemId = newItemId;
        RefreshCount();
    }

    // ---------- Inventory wiring ----------

    private void TrySubscribe()
    {
        PlayerInventory.OnInventoryChange -= HandleInventoryChange; // de-dupe
        PlayerInventory.OnInventoryChange += HandleInventoryChange;
    }

    private void TryUnsubscribe()
    {
        PlayerInventory.OnInventoryChange -= HandleInventoryChange;
    }

    private void HandleInventoryChange(InventoryItem_I changedItem, int delta)
    {
        if (string.IsNullOrEmpty(itemId) || changedItem == null || changedItem.def == null)
            return;

        // Only react if the changed item matches the id this UI cares about
        if (string.Equals(changedItem.def.Id, itemId, StringComparison.Ordinal))
        {
            RefreshCount();
        }
    }

    // ---------- UI refresh & formatting ----------

    private void RefreshCount()
    {
        if (countText == null)
            return;

        if (string.IsNullOrEmpty(itemId))
        {
            countText.text = "0";
            return;
        }

        int count = PlayerInventory.ItemCount(itemId);
        countText.text = FormatAbbrev(count);

        if (hideWhenZero)
        {
            // Hide just the text by default; change to gameObject if you want to hide the whole widget
            countText.gameObject.SetActive(count > 0);
        }
    }

    /// <summary>
    /// Abbreviate numbers:
    ///  - >= 1,000,000,000 -> "B"
    ///  - >= 10,000,000    -> "M"   (starts at 10 million, per your spec)
    ///  - >= 10,000        -> "k"   (starts at 10k)
    ///  - otherwise        -> plain with thousand separators
    /// Uses up to 1 decimal place (e.g., 12.3k, 10.5M, 1.2B).
    /// </summary>
    public static string FormatAbbrev(long value)
    {
        if (value >= 1_000_000_000L) return (value / 1_000_000_000f).ToString("0.#") + "B";
        if (value >= 10_000_000L) return (value / 1_000_000f).ToString("0.#") + "M";
        if (value >= 10_000L) return (value / 1_000f).ToString("0.#") + "k";
        return value.ToString("N0");
    }
}
