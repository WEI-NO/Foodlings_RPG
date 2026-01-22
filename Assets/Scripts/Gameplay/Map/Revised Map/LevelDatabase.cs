using CustomLibrary.References;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum RegionName
{
    Twinreach_Isle,
    Count
}

[CreateAssetMenu(menuName = "Database/Level Database")]
public class LevelDatabase : ScriptableObject
{
    [Header("Data")]
    public List<Level> region_1 = new();

    private List<Level>[] masterlist = new List<Level>[RegionName.Count.ToInt()];

    public void Init()
    {
        masterlist[RegionName.Twinreach_Isle.ToInt()] = new List<Level>(region_1);
    }

    public static int GetTotalLevels(RegionName region)
    {
        var lvlDatabase = MainDatabase.Instance.levelDatabase;

        return MainDatabase.Instance.levelDatabase.masterlist[region.ToInt()].Count;
    }

    public static Level GetLevel(LevelDefinition levelDef)
    {
        var rIndex = levelDef.region.ToInt();
        var lvlDatabase = MainDatabase.Instance.levelDatabase;

        if (rIndex >= lvlDatabase.masterlist.Length)
            return null;

        if (levelDef.index>= lvlDatabase.masterlist[rIndex].Count)
            return null;

        return MainDatabase.Instance.levelDatabase.masterlist[levelDef.region.ToInt()][levelDef.index];
    }
}

public class LevelDefinition
{
    public RegionName region;
    public int index;

    public LevelDefinition(RegionName r, int l)
    {
        region = r;
        index = l;
    }
}
