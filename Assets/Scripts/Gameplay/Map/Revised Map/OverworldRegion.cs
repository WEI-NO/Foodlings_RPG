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
    public GameObject levelStonePrefab;
    public Transform levelStoneContainer;

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

        foreach (var pair in regionNodes)
        {
            LevelNode node = pair.Value;

            GameObject stone = Instantiate(
                levelStonePrefab,
                node.worldPosition + levelStoneOffset,
                Quaternion.identity,
                levelStoneContainer
            );

            stone.name = $"LevelStone_{node.levelIndex}";
        }
    }
}
