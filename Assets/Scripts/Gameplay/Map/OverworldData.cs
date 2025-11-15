using CustomLibrary.References;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OverworldData : MonoBehaviour
{
    public static OverworldData Instance;
    [Header("Overworld Data")]
    public List<int> UnlockedIndex;
    public List<int> NewAddedIndex; // New added region index, used by OverworldLevelController
    public int inprogressLevelIndex = 0;

    public Dictionary<string, Level> completedLevels = new(); // All maps players have completed

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        SceneTransitor.OnSceneTransitionStarted += (sName) =>
        {
            if (sName == "Game Scene")
            {
                var regionManager = RegionManager.Instance;
                if (regionManager)
                {
                    UnlockedIndex = new(regionManager.unlockedRegionIndex);
                }
            }
        };
    }

    public void CompleteLevel(bool unlock = false, List<int> regionToUnlock = null)
    {
        if (unlock && regionToUnlock != null)
        {
            foreach (var i in regionToUnlock)
            {
                if (!UnlockedIndex.Contains(i))
                {
                    if (NewAddedIndex == null) NewAddedIndex = new();
                    NewAddedIndex.Add(i);
                }
            }

        }
    }


    public List<int> PopNewAddedIndex()
    {
        if (NewAddedIndex == null) return null;

        List<int> copy = new(NewAddedIndex);
        NewAddedIndex = null;
        return copy;
    }


    public bool CompletedLevel(Level level)
    {
        if (!completedLevels.TryGetValue(level.LevelName, out var l))
        {
            completedLevels.Add(level.LevelName, level);
            return false;
        }
        else
        {
            return true;
        }
    }

}
