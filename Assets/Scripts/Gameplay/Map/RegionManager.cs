using CustomLibrary.References;
using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RegionManager : MonoBehaviour
{
    public static RegionManager Instance;

    [Header("References")]
    public Tilemap mainTilemap;
    public TilemapDropAnimator dropAnimator;

    [Header("Region Prefabs")]
    public List<LevelProgression> regionPrefabs; // assign in Inspector
    public Dictionary<int, LevelProgression> stepLevelProgression = new();

    public List<int> unlockedRegionIndex = new();
    private bool isReady = false;
    public float cameraLockOnDuration = 4.0f;
    public float cameraStayDuration = 2.0f;
    public float targetZoom;
    public float defaultZoom;

    [Header("Reveal Settings")]
    public float revealDelay = 0.01f;        // (old per-tile delay, you can ignore or remove)
    public float maxDropDuration = 1.5f;     // total time for a region to finish dropping


    private async void Awake()
    {
        Initializer.SetInstance(this);
        isReady = false;
        while (SceneTransitor.Instance == null || SceneTransitor.Instance.isTransitioning)
        {
            await Task.Yield();
        }

        foreach (var r in regionPrefabs)
        {
            if (stepLevelProgression.TryGetValue(r.levelID, out var value))
            {
                // Already exists
            } else
            {
                stepLevelProgression.Add(r.levelID, r);
            }
        }
        isReady = true;
    }



    private async void Start()
    {
        while (SceneTransitor.Instance.isTransitioning || !isReady || (LoadingScreen.Instance != null && LoadingScreen.Instance.IsLoading()))
        {
            await Task.Yield();
        }


        unlockedRegionIndex = new(OverworldData.Instance.UnlockedIndex);
        if (unlockedRegionIndex.Count == 0)
        {
            unlockedRegionIndex.Add(0);
        }
        DisplayUnlocked();
    }

    private void Update()
    {
    }

    private void DisplayUnlocked()
    {
        foreach (var i in unlockedRegionIndex)
        {
            if (stepLevelProgression.TryGetValue(i, out var value))
            {
                DropRegion(value, true);
            }
        }

        StartCoroutine(NewRegionUnlockSequence());

    }

    private IEnumerator NewRegionUnlockSequence()
    {
        // Sequence for newly added map
        var data = OverworldData.Instance;
        if (data)
        {
            var newAdded = data.PopNewAddedIndex();
            if (newAdded != null && newAdded.Count > 0)
            {
                foreach (var i in newAdded)
                {
                    DropRegion(stepLevelProgression[i]);
                    unlockedRegionIndex.Add(i);
                    Region newRegion = stepLevelProgression[i].regionPrefab.GetComponent<Region>();
                    OverworldCameraController.Instance.EnqueuePan(newRegion.overworldLocation, cameraLockOnDuration, cameraStayDuration, targetZoom);
                    yield return new WaitForSeconds(cameraLockOnDuration + cameraStayDuration);
                }
            } else
            {
                yield break;
            }
        }

        Region tempRegion = stepLevelProgression[unlockedRegionIndex[unlockedRegionIndex.Count - 1]].regionPrefab.GetComponent<Region>();
        OverworldCameraController.Instance.EnqueuePan(tempRegion.overworldLocation, cameraLockOnDuration, cameraStayDuration, defaultZoom);
        yield return new WaitForSeconds(cameraLockOnDuration + cameraStayDuration);
    }

    private void DropRegion(LevelProgression region, bool instant = false)
    {
        if (region.regionPrefab == null) return;

        // Instantiate prefab temporarily
        Region tempRegion = Instantiate(region.regionPrefab, new Vector3(0, 100000.0f, 0), Quaternion.identity).GetComponent<Region>();

        Tilemap regionMap = tempRegion.mainTilemap;

        if (!regionMap)
        {
            Debug.LogError("Region prefab has no Tilemap!");
            return;
        }

        foreach (Transform t in tempRegion.levelContainer)
        {
            t.SetParent(null, false);
            t.GetComponent<OverworldLevelController>().regionIndex = region.levelID;
            t.position += new Vector3(tempRegion.overworldLocation.x, tempRegion.overworldLocation.y);
            print(t);
        }

        // Get all cells in the prefab's Tilemap
        BoundsInt bounds = regionMap.cellBounds;
        List<(Vector3Int, TileBase)> cells = new List<(Vector3Int, TileBase)>();

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = regionMap.GetTile(pos);
            if (tile == null) continue;
            cells.Add((pos + new Vector3Int(tempRegion.overworldLocation.x, tempRegion.overworldLocation.y, 0), tile));
        }

        // Drop the tiles onto the main map with stagger
        if (instant || cells.Count == 0)
        {
            dropAnimator.PlaceTilesInstant(cells);
        }
        else
        {
            // Make total reveal time ~ maxDropDuration, regardless of size
            float perTileDelay = maxDropDuration / cells.Count;

            // Optional: clamp to avoid insanely small/large delays
            // perTileDelay = Mathf.Clamp(perTileDelay, 0.001f, 0.05f);

            dropAnimator.PlaceTilesWithDropStagger(cells, perTileDelay);
        }

        Destroy(tempRegion.gameObject);
    }

}

[System.Serializable]
public struct LevelProgression
{
    public GameObject regionPrefab;
    public int levelID;
}
