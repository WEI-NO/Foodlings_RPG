using CustomLibrary.References;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    public static PlayerProgression Instance;

    public RegionProgression[] progression = new RegionProgression[RegionName.Count.ToInt()];

    private void Awake()
    {
        Initializer.SetInstance(this);

        CreateNewProfile();
    }

    public int ProgressedIndex(RegionName region)
    {
        return progression[region.ToInt()].progressedIndex;
    }
    public bool IsLevelComplete(LevelDefinition def)
    {
        int progressed = ProgressedIndex(def.region);
        return def.index < progressed;
    }


    public void OnCompleteLevel(LevelDefinition levelDef)
    {
        RegionProgression prog = progression[levelDef.region.ToInt()];

        if (levelDef.index == prog.progressedIndex)
        {
            int newLevel = progression[levelDef.region.ToInt()].IncrementProgression();
            if (newLevel == levelDef.index)
            {
                // Last level of the region, unlock next region.
            } else
            {
                // Unlock the next level
            }
        }
    }

    private void CreateNewProfile()
    {
        // Creates new region progression
        for (int i = 0; i < progression.Length; i++)
        {
            progression[i] = new RegionProgression((RegionName)i);
        }
    }

}

public class RegionProgression
{
    public RegionName region;
    public int progressedIndex;

    public RegionProgression(RegionName reg)
    {
        region = reg;
        progressedIndex = 0;
    }

    // Increments progressedIndex and return the next index
    public int IncrementProgression()
    {
        var maxLevel = GetMaxLevels();
        progressedIndex = Mathf.Clamp(progressedIndex + 1, 0, maxLevel);

        return progressedIndex;
    }

    private int GetMaxLevels()
    {
        return LevelDatabase.GetTotalLevels(region);
    }
}