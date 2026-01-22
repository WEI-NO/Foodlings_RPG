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

    public static Level GetLevel(RegionName region, int lvlIndex)
    {
        var rIndex = region.ToInt();
        var lvlDatabase = MainDatabase.Instance.levelDatabase;

        if (rIndex >= lvlDatabase.masterlist.Length)
            return null;

        if (lvlIndex >= lvlDatabase.masterlist[rIndex].Count)
            return null;

        return MainDatabase.Instance.levelDatabase.masterlist[region.ToInt()][lvlIndex];
    }
}
