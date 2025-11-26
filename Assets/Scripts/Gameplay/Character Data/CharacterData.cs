using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEngine.GraphicsBuffer;

public enum UnitRank { UnderCooked, HomeCooked, WellCooked, ChefCooked, PremiumCooked, Count}
public enum Faction { Fruit, Veggie, Utensil, Breakfast }

public enum CharacterStatType { HP, PAtk, MAtk, PDef, MDef, AtkRng, AtkSpe, Spe, CD }

[CreateAssetMenu(menuName = "DB/Unit Data")]
public class CharacterData : ScriptableObject
{
    // --- Debug: Max Level Stats Preview ---
    [Header("Debug (Editor Only)")]
    public bool debug_ShowMaxLevelStats = false;

    // Backing fields (hidden, but we can still store values if we want)
    [SerializeField, HideInInspector] private float debug_HP;
    [SerializeField, HideInInspector] private float debug_PAtk;
    [SerializeField, HideInInspector] private float debug_MAtk;
    [SerializeField, HideInInspector] private float debug_PDef;
    [SerializeField, HideInInspector] private float debug_MDef;
    [SerializeField, HideInInspector] private float debug_AtkRng;
    [SerializeField, HideInInspector] private float debug_AtkSpe;
    [SerializeField, HideInInspector] private float debug_Spe;
    [SerializeField, HideInInspector] private float debug_CD;

#if UNITY_EDITOR
    /// <summary>
    /// Recalculate and cache debug stats at max level.
    /// Only used by the custom editor.
    /// </summary>
    public void UpdateDebugStats()
    {
        int lvl = Mathf.Max(1, maxLevel);

        debug_HP = GetRawStat(CharacterStatType.HP, lvl);
        debug_PAtk = GetRawStat(CharacterStatType.PAtk, lvl);
        debug_MAtk = GetRawStat(CharacterStatType.MAtk, lvl);
        debug_PDef = GetRawStat(CharacterStatType.PDef, lvl);
        debug_MDef = GetRawStat(CharacterStatType.MDef, lvl);
        debug_AtkRng = GetRawStat(CharacterStatType.AtkRng, lvl);
        debug_AtkSpe = GetRawStat(CharacterStatType.AtkSpe, lvl);
        debug_Spe = GetRawStat(CharacterStatType.Spe, lvl);
        debug_CD = GetRawStat(CharacterStatType.CD, lvl);
    }

    // Expose the values as read-only properties for the editor
    public float Debug_HP => debug_HP;
    public float Debug_PAtk => debug_PAtk;
    public float Debug_MAtk => debug_MAtk;
    public float Debug_PDef => debug_PDef;
    public float Debug_MDef => debug_MDef;
    public float Debug_AtkRng => debug_AtkRng;
    public float Debug_AtkSpe => debug_AtkSpe;
    public float Debug_Spe => debug_Spe;
    public float Debug_CD => debug_CD;
#endif

    public static readonly float[] RankFusionBaseCost = new float[]
    {
        1.0f, 2.5f, 3.5f, 4f, 5f
    };

    public static readonly float LevelFusionLevelConstant = 2.5f;
    public static readonly int BaseFusionCost = 100;

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
    public float attackRange = 1.5f;
    public float physicalDefense = 1.5f;
    public float magicDefense = 1.5f;

    [Header("Stats Growth")]
    // HP
    [Range(0.01f, 2.0f)] public float growth = 0.5f;
    [Range(0.01f, 2.0f)] public float growthSteepness = 0.9f;

    [Header("Knockback Settings")]
    public float knockbackDuration = 1.2f;
    public float[] healthThreshold = new float[] { 0.5f };

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
                return PolynomialCurve(healthPoint, growth, growthSteepness, level);
            case CharacterStatType.PAtk:
                return PolynomialCurve(physicalDamage, growth, growthSteepness, level);
            case CharacterStatType.MAtk:
                return PolynomialCurve(magicDamage, growth, growthSteepness, level);
            case CharacterStatType.PDef:
                return PolynomialCurve(physicalDefense, growth, growthSteepness, level);
            case CharacterStatType.MDef:
                return PolynomialCurve(magicDefense, growth, growthSteepness, level);
            case CharacterStatType.AtkSpe:
                return attackRate;
            case CharacterStatType.AtkRng:
                return attackRange;
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

    public static int FusionCost(CharacterInstance instance)
    {
        int cost = BaseFusionCost;
        float addition = RankFusionBaseCost[(int)instance.rank] * Mathf.Pow(1 + instance.level, LevelFusionLevelConstant);

        int realCost = cost + Mathf.RoundToInt(addition);
        return realCost;
    }
}

public enum ProgressionType
{
    Linear, Quadratic, Cubic
}
public class ExpProgression
{
    public static float expRequirementGrowth = 1.1f;
    public static int expRequirementRate = 25;
    public ProgressionType type = ProgressionType.Linear;

    public static int GetExpRequirement(int level)
    {
        return Mathf.RoundToInt(expRequirementRate * Mathf.Pow(level, expRequirementGrowth));
    }
}

