using System;
using UnityEngine;


[CreateAssetMenu(menuName = "Game/Upgrade")]
public class UpgradeDef : ScriptableObject
{
    [SerializeField] private string id;    // MUST be unique & never change
    public string Id => id;

    public BattleStatType Type;
    public string Title;
    [TextArea] public string Description;

    [Min(1)] public int MaxLevel = 100;

    [Header("Cost Curve (formula-based, no AnimationCurve)")]
    public FormulaCurve CostCurve = new FormulaCurve
    {
        type = CurveType.Exponential,
        useNormalized = false,   // exponential cost usually uses raw level
        baseValue = 100f,        // base cost
        scale = 100f,            // scales contribution of the exp part
        growth = 1.08f           // +8% per level
    };

    [Header("Value Curve (formula-based, no AnimationCurve)")]
    public FormulaCurve ValueCurve = new FormulaCurve
    {
        type = CurveType.Power,
        useNormalized = true,    // value often benefits from normalized (smooth across maxLevel)
        baseValue = 0f,
        scale = 1f,
        exponent = 1f            // linear by default; try 0.5 for diminishing, 2 for accelerating
    };

    public int CostForLevel(int nextLevel)
    {
        float v = CostCurve != null ? CostCurve.Evaluate(nextLevel, MaxLevel) : 0f;
        return Mathf.Max(0, Mathf.RoundToInt(v));
    }

    public float ValueAtLevel(int level)
    {
        return ValueCurve != null ? ValueCurve.Evaluate(level, MaxLevel) : 0f;
    }
}


public enum CurveType { Linear, Exponential, Power, Step, Logistic }

[Serializable]
public class FormulaCurve
{
    [Header("General")]
    public CurveType type = CurveType.Linear;

    [Tooltip("If true, maps level 1..maxLevel -> t in [0..1]. If false, uses raw level.")]
    public bool useNormalized = true;

    [Tooltip("Added at the end: final = baseValue + scale * f(x).")]
    public float baseValue = 0f;

    [Tooltip("Multiplier for the shape output f(x).")]
    public float scale = 1f;

    [Header("Linear / Power")]
    [Tooltip("For Power: exponent n (e.g., 2 = quadratic, 3 = cubic). For Linear this is ignored.")]
    public float exponent = 1f;

    [Header("Exponential")]
    [Tooltip("Exponential growth factor > 1 (e.g., 1.08 = +8%/step).")]
    public float growth = 1.1f;

    [Header("Step")]
    [Tooltip("Every N levels add one step.")]
    public int stepEvery = 5;

    [Tooltip("Value of each step before 'scale' is applied (usually leave 1, adjust with scale).")]
    public float stepAmount = 1f;

    [Header("Logistic")]
    [Tooltip("Steepness of S-curve (higher = steeper).")]
    public float steepness = 10f;

    [Tooltip("Midpoint in normalized space (0..1). 0.5 means the curve’s inflection at mid-level.")]
    public float midpoint = 0.5f;

    /// <summary>
    /// Evaluate curve at a given level (1..maxLevel).
    /// </summary>
    public float Evaluate(int level, int maxLevel)
    {
        if (maxLevel < 1) maxLevel = 1;
        level = Mathf.Clamp(level, 0, maxLevel);

        float x = useNormalized
            ? Mathf.InverseLerp(1f, maxLevel, level)        // t in [0..1]
            : (float)level;                                  // raw level

        float y = 0f;

        switch (type)
        {
            case CurveType.Linear:
                // f(x) = x
                y = x;
                break;

            case CurveType.Power:
                // f(x) = x^n
                y = Mathf.Pow(x, Mathf.Max(0f, exponent));
                break;

            case CurveType.Exponential:
                // f(x) = growth^x - 1
                // If normalized, x in [0..1] gives smooth early growth; if raw, it grows quickly per level.
                y = Mathf.Pow(Mathf.Max(1.000001f, growth), x) - 1f;
                break;

            case CurveType.Step:
                // f(x) = floor(level/stepEvery) * stepAmount   (if useNormalized, map back to level count)
                float lvl = useNormalized ? Mathf.Lerp(1f, maxLevel, x) : x;
                y = Mathf.Floor(lvl / Mathf.Max(1, stepEvery)) * stepAmount;
                break;

            case CurveType.Logistic:
                // f(x) = 1 / (1 + exp( -k * (t - midpoint) )) mapped to [0..1]
                // Works best with normalized input.
                float t = useNormalized ? x : Mathf.Clamp01((x - 1f) / Mathf.Max(1, maxLevel - 1));
                y = 1f / (1f + Mathf.Exp(-steepness * (t - Mathf.Clamp01(midpoint))));
                break;
        }

        return baseValue + y * scale;
    }
}
