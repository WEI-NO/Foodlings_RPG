using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    public List<string> charactersInLevel = new();
    public List<int> characterLevels = new();
    public List<CharacterInstance> characterData = new();
    public List<Wave> waves = new();

    public List<Wave> unstartedWaves = new();
    public List<Wave> inprogressWaves = new();

    public float initialDelay = 5.0f;

    [Header("Level Settings")]
    public string LevelName;
    [SerializeField] private float timePassed;
    

    public bool ready = false;

    [Header("Level Reward")]
    public List<RewardEntry> firstClearRewards = new();
    public List<RewardEntry> clearRewards = new();

    public async void Initialize()
    {
        ready = false;
        timePassed = 0;
        unstartedWaves = new(waves);

        while (!CharacterDatabase.Instance.IsReady)
        {
            await Task.Yield();
        }

        // Load Prefabs for characters
        int i = 0;
        foreach (var id in charactersInLevel)
        {
            var data = CharacterDatabase.Instance.GetById(id);
            if (data == null)
            {
                Debug.LogWarning($"Character: {id} not found");
                i++;       
                continue;
            }

            CharacterInstance newInstance = new CharacterInstance();
            newInstance.ResetCharacter(data);
            newInstance.SetLevel(characterLevels[i]);
            characterData.Add(newInstance);
            i++;
        }

        ready = true;
    }

    public void UpdateLevel(float dt)
    {
        if (!ready) return;

        if (timePassed >= initialDelay)
        {
            // Unstarted Waves
            for (int i = unstartedWaves.Count - 1; i >= 0; i--)
            {
                var wave = unstartedWaves[i];
                if (wave.startSeconds <= timePassed)
                {
                    inprogressWaves.Add(wave);
                    unstartedWaves.RemoveAt(i);
                }
            }

            // Inprogress Waves
            for (int i = inprogressWaves.Count - 1; i >= 0; i--)
            {
                var wave = inprogressWaves[i];
                if (wave.UpdateWave(dt))
                {
                    if (wave.characterIndex < characterData.Count)
                    {
                        // Spawn For Enemy
                        Debug.Log("Spawning");
                        var spawnedActor = Instantiate(characterData[wave.characterIndex].baseData.unitPrefab);
                        spawnedActor.transform.position = MapController.Instance.spawnedEnemyBase.GetSpawnPoint();
                        Debug.Log(spawnedActor.transform.position);
                        var entity = spawnedActor.GetComponent<CharacterEntity>();
                        entity.SetTeam(Team.Hostile);
                        entity.SetCharacterInstance(characterData[wave.characterIndex]);
                        CharacterContainer.Instance.RegisterUnit(entity);
                    }
                }
                if (wave.abort)
                {
                    inprogressWaves.RemoveAt(i);
                }
            }
        }

        timePassed += dt;
    }


}

[System.Serializable]
public class Wave
{
    [Header("Timeline Settings")]
    public float startSeconds;

    [Header("Spawn Settings")]
    public int characterIndex;

    [Header("Mode Toggles")]
    public bool single;
    public bool burst;
    public bool repeater;

    [System.Serializable]
    public struct BurstOptions
    {
        public int amount;
        public float delay;
    }
    public BurstOptions burstOptions;

    [System.Serializable]
    public struct RepeaterOptions
    {
        public bool spawnOnStart;
        public float interval;
        public float duration;
        public bool useRandomInterval;
        public Vector2 randomInterval;
    }
    public RepeaterOptions repeaterOptions;
    private float repeaterRandomTimer = 0.0f;
    private bool repeaterRandomSpawn = false;


    // Internal runtime fields (hidden in inspector)
    [HideInInspector] public bool abort = false;
    [HideInInspector] public int spawnedSoFar = 0;
    [HideInInspector] public float timePassed = 0;

    public Wave()
    {
        repeaterRandomTimer = 0.0f;
    }

    public bool UpdateWave(float dt)
    {
        if (abort) return false;

        if (single) return SingleUpdate(dt);
        if (burst) return BurstUpdate(dt);
        if (repeater) return RepeaterUpdate(dt);

        timePassed += dt;
        return false;
    }

    bool SingleUpdate(float dt)
    {
        abort = true;
        timePassed += dt;
        return true;
    }

    bool RepeaterUpdate(float dt)
    {
        if (timePassed >= repeaterOptions.duration)
        {
            abort = true;
            return false;
        }

        // Spawn On Start
        if (timePassed == 0 && repeaterOptions.spawnOnStart)
        {
            timePassed += dt;
            return true;
        }

        
        float threshold = (spawnedSoFar + 1) * repeaterOptions.interval;

        // Random Interval
        if (repeaterOptions.useRandomInterval && !repeaterRandomSpawn)
        {
            float randomInterval = Random.Range(repeaterOptions.randomInterval.x, repeaterOptions.randomInterval.y);
            repeaterRandomTimer += randomInterval;
            repeaterRandomSpawn = true;
        }

        if (repeaterOptions.useRandomInterval && timePassed >= repeaterRandomTimer)
        {
            repeaterRandomSpawn = false;
            timePassed += dt;
            spawnedSoFar++;
            return true;
        }


        if (timePassed >= threshold && !repeaterOptions.useRandomInterval)
        {
            timePassed += dt;
            spawnedSoFar++;
            return true;
        }

        timePassed += dt;
        return false;
    }

    bool BurstUpdate(float dt)
    {
        float threshold = (spawnedSoFar + 1) * burstOptions.delay;

        if (spawnedSoFar >= burstOptions.amount)
        {
            abort = true;
            timePassed += dt;
            return false;
        }

        if (timePassed >= threshold)
        {
            timePassed += dt;
            spawnedSoFar++;
            return true;
        }

        timePassed += dt;
        return false;
    }
}

[System.Serializable]
public class RewardEntry
{
    public string rewardID;
    [SerializeField] int quantity;

    public bool useQuantityRange;
    public Vector2Int quantityRange = new Vector2Int();

    public bool rolled = false;

    public RewardEntry()
    {
        rolled = false;
        Roll();
    }

    public void Roll()
    {
        if (useQuantityRange)
        {
            quantity = Random.Range(quantityRange.x, quantityRange.y);
        }
        rolled = true;
    }

    public int GetQuantity()
    {
        return quantity;
    }

}