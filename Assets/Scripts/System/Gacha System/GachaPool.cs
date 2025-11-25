using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Gacha/Gacha Pool")]
public class GachaPool : ScriptableObject
{
    [Header("Pool Settings")]
    public List<string> rankone_pool = new(); 
    public List<string> ranktwo_pool = new(); 
    public List<string> rankthree_pool = new(); 
    public List<string> rankfour_pool = new(); 
    public List<string> rankfive_pool = new();

    public List<List<string>> pools = new();

    public int[] rankedChances = new int[(int)UnitRank.Count];
    public float[] chances = new float[(int)UnitRank.Count];

    public string RollCharacter()
    {
        CalculateChances();
        CombinePools();

        float chance = Random.Range(0.0f, 1.0f);

        int selectedRank = 0;
        for (int i = 0; i < chances.Length; i++)
        {
            if (chance <= chances[i])
            {
                selectedRank = i;
            }
        }

        int characterIndex = Random.Range(0, pools[selectedRank].Count);

        return pools[selectedRank][characterIndex];
    }

    public void CombinePools()
    {
        pools = new()
        { 
            rankone_pool, ranktwo_pool, rankthree_pool, rankfour_pool, rankfive_pool 
        };
    }

    public void CalculateChances()
    {
        // 1. Calculate total weight
        int total = 0;
        for (int i = 0; i < rankedChances.Length; i++)
        {
            total += rankedChances[i];
        }

        if (total <= 0)
        {
            Debug.LogWarning("Gacha chances total is zero — check rankedChances!");
            for (int i = 0; i < chances.Length; i++)
                chances[i] = 0f;
            return;
        }

        // 2. Normalize each chance
        for (int i = 0; i < rankedChances.Length; i++)
        {
            chances[i] = (float)rankedChances[i] / total;
        }
    }

}
