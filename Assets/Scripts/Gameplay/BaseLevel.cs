using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu]
public class Level : ScriptableObject
{
    public List<string> charactersInLevel = new();
    public List<int> characterLevels = new();
    [HideInInspector] public List<CharacterInstance> characterInstances = new();
    public List<Wave> waves = new();

    [HideInInspector] public List<Wave> unstartedWaves = new();
    [HideInInspector] public List<Wave> inprogressWaves = new();

    public MapLength mapLength;

    public float initialDelay = 5.0f;

    [Header("Level Settings")]
    public string LevelName;
    private float timePassed;

    [HideInInspector] public bool ready = false;

    [Header("Level Reward")]
    public List<RewardEntry> firstClearRewards = new();
    public List<RewardEntry> clearRewards = new();

    public async void Initialize()
    {
        ready = false;
        timePassed = 0;
        unstartedWaves = new(waves);

        while (!GameBoostrapper.Instance.DataLoaded)
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
            characterInstances.Add(newInstance);
            i++;
        }


        ready = true;
    }

    public void UpdateLevel(float dt)
    {
        if (!ready) return;

        timePassed += dt;

        if (timePassed <= initialDelay)
        {
            return;
        }

        // Scan Through Unstarted Waves
        for (int i = unstartedWaves.Count - 1;  i >= 0; i--)
        {
            var wave = unstartedWaves[i];
            if (wave.startTime <= timePassed)
            {
                inprogressWaves.Add(wave);
                unstartedWaves.RemoveAt(i);
            }
        }

        // Inprogress Waves
        for (int i = inprogressWaves.Count - 1; i >= 0; i--)
        {
            var wave = inprogressWaves[i];

            if (!wave.Active())
            {
                inprogressWaves.RemoveAt(i);
                continue;
            }

            if (wave.UpdateWave(dt))
            {
                if (wave.characterIndex < characterInstances.Count)
                {
                    // Spawn For Enemy
                    var spawnedActor = Instantiate(characterInstances[wave.characterIndex].baseData.unitPrefab);
                    // Set up actor
                    spawnedActor.transform.position = MapController.Instance.spawnedEnemyBase.GetSpawnPoint();
                    var entity = spawnedActor.GetComponent<CharacterEntity>();
                    entity.SetTeam(Team.Hostile);
                    entity.SetCharacterInstance(characterInstances[wave.characterIndex]);
                    CharacterContainer.Instance.RegisterUnit(entity);
                    wave.RegisterUnit(entity);
                }
            }
        }
    }

    public int GetAverageLevel()
    {
        int total = 0;
        foreach (var l in characterLevels)
        {
            total += l;
        }
        int totalCount = (characterLevels.Count == 0 ? 1 : characterLevels.Count);
        int average = Mathf.RoundToInt((float)total / totalCount);
        return average;
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