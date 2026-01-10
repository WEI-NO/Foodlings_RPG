using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gacha/Star Rarity Table")]
public class StarRarityTable : ScriptableObject
{
    public List<StarChanceRow> rows = new();

    public UnitRank RollStar(int totalStage)
    {
        foreach (var row in rows)
        {
            if (totalStage >= row.minTotal && totalStage <= row.maxTotal)
            {
                return RollFromRow(row);
            }
        }

        Debug.LogWarning($"No star table found for totalStage={totalStage}");
        return UnitRank.UnderCooked;
    }

    public int GetCookTime(int totalStage)
    {
        foreach (var row in rows)
        {
            if (totalStage >= row.minTotal && totalStage <= row.maxTotal)
            {
                return row.cookTime_s;
            }
        }

        Debug.LogWarning($"No cook time found for totalStage={totalStage}, using default.");
        return 30; // fallback default
    }


    private UnitRank RollFromRow(StarChanceRow row)
    {
        float roll = Random.Range(0f, 100f);
        float cumulative = 0f;

        cumulative += row.oneStar;
        if (roll < cumulative) return UnitRank.UnderCooked;

        cumulative += row.twoStar;
        if (roll < cumulative) return UnitRank.HomeCooked;

        cumulative += row.threeStar;
        if (roll < cumulative) return UnitRank.WellCooked;

        cumulative += row.fourStar;
        if (roll < cumulative) return UnitRank.ChefCooked;

        return UnitRank.PremiumCooked;
    }
}

[System.Serializable]
public class StarChanceRow
{
    [Range(0, 15)]
    public int minTotal;

    [Range(0, 15)]
    public int maxTotal;

    [Header("Star Chances (must sum to 100)")]
    [Range(0, 100)] public float oneStar;
    [Range(0, 100)] public float twoStar;
    [Range(0, 100)] public float threeStar;
    [Range(0, 100)] public float fourStar;
    [Range(0, 100)] public float fiveStar;

    public int cookTime_s = 30;
}
