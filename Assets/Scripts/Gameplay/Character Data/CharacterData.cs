using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum UnitRarity { Common, Rare, Epic, Legendary }
public enum Faction { Fruit, Veggie, Utensil, Breakfast }

[CreateAssetMenu(menuName = "DB/Unit Data")]
public class CharacterData : ScriptableObject
{
    [Header("Exp Requirements")]
    public static List<ExpProgression> expProgressions = new()
    {
        new ExpProgression
        {
            levelRange = new Vector2Int(1, 20),
            expRequirementRate = 50,
            type = ProgressionType.Linear
        },
        new ExpProgression
        {
            levelRange = new Vector2Int(21, 60),
            expRequirementRate = 75,
            type = ProgressionType.Quadratic
        },
        new ExpProgression
        {
            levelRange = new Vector2Int(61, 100),
            expRequirementRate = 100,
            type = ProgressionType.Cubic
        }
    };

    [Header("Identity")]
    public string id;
    public string displayName;
    public UnitRarity rarity;
    public Faction faction;

    [Header("Economy")]
    public int summonCost;
    public float cooldownSec;

    [Header("Stats (base at level 1)")]
    public float hp = 100f;
    public float damage = 10f;
    public float attackRate = 1.0f;  // attacks per second at level 1
    public float moveSpeed = 3.5f;
    public float range = 1.5f;
    public float defense = 1.5f;


    [Header("Assets (Addressables)")]
    public GameObject unitPrefab;
    public Sprite unitSprite;

    [Header("Progression (multipliers)")]
    // Curves should output ~1.0 at L=1 and grow from there
    public AnimationCurve hpGrowth;      // multiplier vs. level
    public AnimationCurve attackGrowth;  // multiplier vs. level (apply to DAMAGE)
    public AnimationCurve attackRateGrowth; // optional: multiplier for attackRate

    [Header("Level Settings")]
    public int maxLevel = 30;

    // --- Helpers ---
    private int ClampLevel(int level) => Mathf.Clamp(level, 1, Mathf.Max(1, maxLevel));

    // Evaluate curve on normalized level t in [0,1]
    // t = 0 at level 1, t = 1 at maxLevel
    private float EvalCurve01(AnimationCurve curve, int level)
    {
        if (curve == null || curve.length == 0) return 1f;
        int L = ClampLevel(level);
        float t = (maxLevel <= 1) ? 0f : (L - 1f) / (maxLevel - 1f);
        return Mathf.Max(0f, curve.Evaluate(t)); // prevent negative multipliers
    }

    // --- Public API ---
    public float AttackCooldown() =>
        1f / Mathf.Clamp(attackRate, 0.0001f, float.MaxValue);

    public float GetHP(int level)
    {
        float m = EvalCurve01(hpGrowth, level);     // multiplier
        return Mathf.Max(1f, hp * m);
    }

    public float GetDamage(int level)
    {
        float m = EvalCurve01(attackGrowth, level); // multiplier
        return Mathf.Max(0f, damage * m);
    }

    public static int GetRequiredExp(int level)
    {
        foreach (var i in expProgressions)
        {
            if (level >= i.levelRange.x && level <= i.levelRange.y)
            {
                return i.GetExpRequirement(level);
            }
        }
        return 100;
    }
}

public enum ProgressionType
{
    Linear, Quadratic, Cubic
}
public class ExpProgression
{
    public Vector2Int levelRange = new Vector2Int(0, 10);
    public int expRequirementRate = 50;
    public ProgressionType type = ProgressionType.Linear;

    public int GetExpRequirement(int level)
    {
        switch (type)
        {
            case ProgressionType.Linear:
                return expRequirementRate* level;
            case ProgressionType.Quadratic:
                return expRequirementRate * Mathf.RoundToInt(Mathf.Pow(level, 2));
            case ProgressionType.Cubic:
                return expRequirementRate * Mathf.RoundToInt(Mathf.Pow(level, 3));
        }
        return 0;
    }
}
