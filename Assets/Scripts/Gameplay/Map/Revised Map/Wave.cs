using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave
{
    [Header("================= WAVE SETTINGS =================")]

    [SerializeField, TextArea] private string Notes;

    [Header("Timing")]
    public float startTime; // Relative start time
    [Tooltip("0 : infinite")] public float duration = 1; // Duration of this wave
    public bool spawnOnStart = true;
    private bool spawnOnStart_Triggered = false;

    [Header("Spawn")]
    public int characterIndex; // Character index from the levelStone

    [Header("Spawn Control")]
    private List<BaseEntity> spawnedEnemies = new();
    [Range(1, 100)] public int maxDeployed; // Maximum deployment count for this current wave. (min.1)

    [Header("Spawn Control - Fixed Interval")]
    public Vector2 spawnIntervalRange = new Vector2(0, 1); // Random interval between spawns


    // Private
    private float internal_timer = 0.0f;
    private float lifetimer = 0.0f;

    public void ResetWave()
    {
        internal_timer = 0.0f;
        spawnOnStart_Triggered = false;
        ResetTimer();
    }

    public bool UpdateWave(float dt)
    {
        lifetimer += dt;

        // Prevent spawn when reached maxDeployed
        if (!CanSpawn())
        {
            return false;
        }

        internal_timer -= dt;
        if (!spawnOnStart_Triggered && spawnOnStart)
        {
            ResetTimer();
            spawnOnStart_Triggered = true;
            return true;
        }
        
        if (internal_timer <= 0)
        {
            ResetTimer();
            return true;
        }

        return false;
    }

    public bool CanSpawn()
    {
        return spawnedEnemies.Count < Mathf.Clamp(maxDeployed, 1, maxDeployed);
    }
    
    public bool Active()
    {
        return lifetimer < duration;
    }
    
    public void RegisterUnit(BaseEntity enemy)
    {
        if (enemy == null) return;
        spawnedEnemies.Add(enemy);
        enemy.OnEntityDead += (i) => { if (spawnedEnemies.Contains(i)) { spawnedEnemies.Remove(i); } };
    }

    private void ResetTimer()
    {
        internal_timer = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
    }

}

public class DynamicSpawnInterval
{
    public float percentageThreshold;
    public Vector2 intervalRange;
}
