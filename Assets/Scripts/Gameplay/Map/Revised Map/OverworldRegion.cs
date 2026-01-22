using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

// As prefab
public class OverworldRegion : MonoBehaviour
{
    [Header("Region Settings")]
    public Dictionary<int, LevelNode> regionNodes;

    [Header("Tilemap Scanning")]
    public LevelMarkerScanner markerScanner;
    public DirectedLevelGraph directedGraph;

    [Header("Level Stones")]
    public LevelStone levelStonePrefab;
    public Transform levelStoneContainer;
    public List<LevelStone> orderedLevelStone; // A list of levelstone ordered by level index

    [Header("Level Structure")]
    public RegionLevelList levelList;
    public RegionName region;

    public Vector3 levelStoneOffset = new Vector3(0.25f, 0.25f, 0.0f);

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        regionNodes = DLGBuilder.Build(directedGraph, markerScanner.Scan());
        SpawnLevelStones();
    }

    void SpawnLevelStones()
    {
        if (levelStonePrefab == null)
        {
            Debug.LogError("OverworldRegion levelStonePrefab is not assigned");
            return;
        }

        // Create a new list for orderedLevelStone
        orderedLevelStone = new();

        // Get Player Progression Data
        var pp = PlayerProgression.Instance;
        int unlockedIndex = pp.ProgressedIndex(region); // Up to but not including this level is unlocked
        // Clamp to 1 to always allow level 1
        unlockedIndex = Mathf.Clamp(unlockedIndex, 1, unlockedIndex);

        // Loop through each pair (
        foreach (var pair in regionNodes)
        {
            LevelNode node = pair.Value;

            LevelStone stone = Instantiate(
                levelStonePrefab,
                node.worldPosition + levelStoneOffset,
                Quaternion.identity,
                levelStoneContainer
            );

            orderedLevelStone.Add(stone);

            // Disable if it is locked
            stone.gameObject.SetActive(pair.Key < unlockedIndex);

            stone.Init(region, node.levelIndex);
            stone.name = $"LevelStone_{node.levelIndex}";
        }
    }
}
