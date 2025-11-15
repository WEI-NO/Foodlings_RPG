using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    public List<string> charactersInLevel = new();
    public List<CharacterData> characterData = new();
    public List<Wave> waves = new();

    public List<Wave> unstartedWaves = new();
    public List<Wave> inprogressWaves = new();

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
        foreach (var id in charactersInLevel)
        {
            var data = CharacterDatabase.Instance.GetById(id);
            if (data == null)
            {
                Debug.LogWarning($"Character: {id} not found");
                continue;
            }
            characterData.Add(data);
        }

        ready = true;
    }

    public void UpdateLevel(float dt)
    {
        if (!ready) return;

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
        for (int i = inprogressWaves.Count - 1; i >= 0 ; i--)
        {
            var wave = inprogressWaves[i];
            if (wave.UpdateWave(dt))
            {
                if (wave.characterIndex < characterData.Count)
                {
                    // Spawn For Enemy
                    Debug.Log("Spawning");
                    var spawnedActor = Instantiate(characterData[wave.characterIndex].unitPrefab);
                    spawnedActor.transform.position = MapController.Instance.spawnedEnemyBase.GetSpawnPoint();
                    Debug.Log(MapController.Instance.spawnedEnemyBase.GetSpawnPoint());
                    var entity = spawnedActor.GetComponent<CharacterEntity>();
                    entity.SetTeam(Team.Hostile);
                    CharacterContainer.Instance.RegisterUnit(entity);
                }
            }
            if (wave.startSeconds + wave.duration <= timePassed)
            {
                inprogressWaves.RemoveAt(i);
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
    public float duration;
    public bool single = false;

    [Header("Spawn Settings")]
    public int characterIndex;

    [Header("Repeater Settings")]
    public bool repeater = false;
    public bool spawnOnStart = true;
    public float interval = 5;
    private int spawnedSoFar = 0;

    public float timePassed;
    
    public bool UpdateWave(float dt)
    {
        if (repeater)
        {
            if (timePassed == 0 && spawnOnStart)
            {
                return true;
            } else {
                float threshold = (spawnedSoFar + 1) * interval;
                if (timePassed >= threshold)
                {
                    spawnedSoFar++;
                    return true;
                }
            }
        } else
        {
            if (single)
            {
                return true;
            }
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