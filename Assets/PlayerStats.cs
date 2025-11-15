using CustomLibrary.References;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [Header("Stats")]
    public List<StatsDef> availableStats = new();
    public Dictionary<string, float> playerStats = new();

    [Header("Upgrades")]
    public Dictionary<string, int> upgradeLevels = new();

    private void Awake()
    {
        Initializer.SetInstance(this);

        foreach (StatsDef stats in availableStats)
        {
            playerStats.Add(stats.Id, stats.baseValue);
        }
    }

    private void Update()
    {

    }

    public float GetValue(string id)
    {
        if (playerStats.TryGetValue(id, out var value)) return value;
        return 0.0f;
    }


    public int GetCurrentLevel(string id)
    {
        if (!upgradeLevels.TryGetValue(id, out int level))
            return 0; // default if never upgraded
        return level;
    }

    public void Upgrade(string id)
    {
        if (upgradeLevels.TryGetValue(id, out int level))
        {
            upgradeLevels[id]++;
        } else
        {
            if (MainDatabase.Instance.upgradeDatabase.Contains(id))
            {
                upgradeLevels.Add(id, 1);
            }
        }
        UpdateStats();
    }

    public void UpdateStats()
    {
        foreach (var i in upgradeLevels)
        {
            if (MainDatabase.Instance.upgradeDatabase.Contains(i.Key))
            {
                var def = MainDatabase.Instance.upgradeDatabase.Get(i.Key);
                int index = def.Type.ToInt();
                if (playerStats.TryGetValue(i.Key, out var statDef))
                {
                    var stat = GetStats(i.Key);
                    var newValue = stat.CalculateValue(def, i.Value);
                    
                    playerStats[i.Key] = newValue;
                    print($"{i.Key} new value: {newValue}");
                }
            }
        }
    }
    
    public StatsDef GetStats(string id)
    {
        foreach (var i in availableStats)
        {
            if (i.Id == id)
            {
                return i;
            }
        }
        return null;
    }
}
