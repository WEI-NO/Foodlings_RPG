using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Sprite Database")]
public class SpriteDatabase : ScriptableObject
{
    [Header("All upgrade definitions in game")]
    public List<SpriteContainer> sprites = new List<SpriteContainer>();

    // Runtime lookup table
    private Dictionary<string, SpriteContainer> lookup;

    /// <summary>
    /// Returns the upgrade definition by ID.
    /// </summary>
    public SpriteContainer Get(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        EnsureLookup();
        lookup.TryGetValue(id, out var def);
        return def;
    }

    /// <summary>
    /// Returns all upgrade definitions.
    /// </summary>
    public List<SpriteContainer> GetAll()
    {
        EnsureLookup();
        return sprites;
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
    public bool TryGet(string id, out SpriteContainer def)
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

        lookup = new Dictionary<string, SpriteContainer>();

        foreach (var sp in sprites)
        {
            if (sp == null)
            {
                Debug.LogWarning("[UpgradeDatabase] Null entry in list.");
                continue;
            }

            // Ensure unique, stable ID on the UpgradeDef asset
            if (string.IsNullOrEmpty(sp.Id))
            {
                Debug.LogWarning($"[UpgradeDatabase] Upgrade missing ID: {sp}");
                continue;
            }

            if (lookup.ContainsKey(sp.Id))
            {
                Debug.LogWarning($"[UpgradeDatabase] Duplicate Upgrade ID found: {sp.Id}");
                continue;
            }

            lookup.Add(sp.Id, sp);
        }
    }

    // Extra

    public static string GetRoleID(Role role)
    {
        switch (role)
        {
            case Role.Fighter:
                return "fighter_icon";
            case Role.Tank:
                return "tank_icon";
            case Role.Magic:
                return "magic_icon";
            case Role.Support:
                return "support_icon";
            default:
                return "fighter_icon";
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

[System.Serializable]
public class SpriteContainer
{
    public Sprite sprite;
    public string Id;
}
