using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public enum UnitRank { UnderCooked, HomeCooked, WellCooked, ChefCooked, PremiumCooked}
public enum Faction { Fruit, Veggie, Utensil, Breakfast }

public enum CharacterStatType { HP, PAtk, MAtk, PDef, MDef, AtkSpe, Spe, CD }

[CreateAssetMenu(menuName = "DB/Unit Data")]
public class CharacterData : ScriptableObject
{
    [Header("Identity")]
    public string id;
    public string displayName;
    public UnitRank baseRank;
    public Faction faction;

    [Header("Economy")]
    public int summonCost;
    public float cooldownSec;

    [Header("Stats (base at level 1)")]
    public float healthPoint = 100f;
    public float physicalDamage = 10f;
    public float magicDamage = 10f;
    public float attackRate = 1.0f;  // attacks per second at level 1
    public float moveSpeed = 3.5f;
    public float range = 1.5f;
    public float physicalDefense = 1.5f;
    public float magicDefense = 1.5f;

    [Header("Stats Growth")]
    // HP
    [Range(0.01f, 2.0f)] public float hpGrowth = 0.8f;
    [Range(0.01f, 2.0f)] public float hpGrowthSteepness = 1.35f;

    [Range(0.01f, 2.0f)] public float atkGrowth = 0.8f;
    [Range(0.01f, 2.0f)] public float atkGrowthSteepness = 1.35f;

    [Range(0.01f, 2.0f)] public float defGrowth = 0.8f;
    [Range(0.01f, 2.0f)] public float defGrowthSteepness = 1.35f;


    [Header("Assets (Addressables)")]
    public GameObject unitPrefab;
    public Sprite unitSprite;

    [Header("Level Settings")]
    public int maxLevel = 30;

    // --- Helpers ---
    private int ClampLevel(int level) => Mathf.Clamp(level, 1, Mathf.Max(1, maxLevel));

    // --- Public API ---
    public float AttackCooldown() =>
        1f / Mathf.Clamp(attackRate, 0.0001f, float.MaxValue);

    public float GetRawStat(CharacterStatType type, int level)
    {
        switch (type)
        {
            case CharacterStatType.HP:
                return PolynomialCurve(healthPoint, hpGrowth, hpGrowthSteepness, level);
            case CharacterStatType.PAtk:
                return PolynomialCurve(physicalDamage, atkGrowth, atkGrowthSteepness, level);
            case CharacterStatType.MAtk:
                return PolynomialCurve(magicDamage, atkGrowth, atkGrowthSteepness, level);
            case CharacterStatType.PDef:
                return PolynomialCurve(physicalDefense, defGrowth, defGrowthSteepness, level);
            case CharacterStatType.MDef:
                return PolynomialCurve(magicDefense, defGrowth, defGrowthSteepness, level);
            case CharacterStatType.AtkSpe:
                return attackRate;
            case CharacterStatType.Spe:
                return moveSpeed;
            case CharacterStatType.CD:
                return cooldownSec;
            default:
                return 0;
        }
    }

    private float PolynomialCurve(float baseValue, float growth, float steepness, int level)
    {
        return baseValue * Mathf.Pow((1 + steepness * (level - 1)), growth);
    }

    public static int GetRequiredExp(int level)
    {
        return ExpProgression.GetExpRequirement(level);
    }
}

public enum ProgressionType
{
    Linear, Quadratic, Cubic
}
public class ExpProgression
{
    public static float expRequirementGrowth = 1.2f;
    public static int expRequirementRate = 50;
    public ProgressionType type = ProgressionType.Linear;

    public static int GetExpRequirement(int level)
    {
        return Mathf.RoundToInt(expRequirementRate * Mathf.Pow(level, expRequirementGrowth));
    }
}
