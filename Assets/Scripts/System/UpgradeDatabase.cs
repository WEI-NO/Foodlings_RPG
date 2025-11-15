using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Upgrade Database")]
public class UpgradeDatabase : ScriptableObject
{
    [Header("All upgrade definitions in game")]
    public List<UpgradeDef> upgrades = new List<UpgradeDef>();

    // Runtime lookup table
    private Dictionary<string, UpgradeDef> lookup;

    /// <summary>
    /// Returns the upgrade definition by ID.
    /// </summary>
    public UpgradeDef Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        EnsureLookup();
        lookup.TryGetValue(id, out var def);
        return def;
    }

    /// <summary>
    /// Returns all upgrade definitions.
    /// </summary>
    public List<UpgradeDef> GetAll()
    {
        EnsureLookup();
        return upgrades;
    }

    /// <summary>
    /// Check if database contains this upgrade ID.
    /// </summary>
    public bool Contains(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        EnsureLookup();
        return lookup.ContainsKey(id);
    }

    /// <summary>
    /// Try get (no alloc on miss).
    /// </summary>
    public bool TryGet(string id, out UpgradeDef def)
    {
        def = null;
        if (string.IsNullOrEmpty(id)) return false;
        EnsureLookup();
        return lookup.TryGetValue(id, out def);
    }

    /// <summary>
    /// Build lookup if needed.
    /// </summary>
    private void EnsureLookup()
    {
        if (lookup != null) return;

        lookup = new Dictionary<string, UpgradeDef>();

        foreach (var up in upgrades)
        {
            if (up == null)
            {
                Debug.LogWarning("[UpgradeDatabase] Null entry in list.");
                continue;
            }

            // Ensure unique, stable ID on the UpgradeDef asset
            if (string.IsNullOrEmpty(up.Id))
            {
                Debug.LogWarning($"[UpgradeDatabase] Upgrade missing ID: {up.name}");
                continue;
            }

            if (lookup.ContainsKey(up.Id))
            {
                Debug.LogWarning($"[UpgradeDatabase] Duplicate Upgrade ID found: {up.Id}");
                continue;
            }

            lookup.Add(up.Id, up);
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
