using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelMarkerScanner : MonoBehaviour
{
    [Header("Tilemap Reference")]
    [Tooltip("Tilemap that contains level marker tiles and is used only for logic")]
    public Tilemap markerTilemap;

    public Dictionary<int, Vector3> Scan()
    {
        var levelPositions = new Dictionary<int, Vector3>();

        if (markerTilemap == null)
        {
            Debug.LogError("LevelMarkerScanner markerTilemap is not assigned");
            return levelPositions;
        }

        List<Vector3Int> markerCells = new List<Vector3Int>();

        foreach (var cell in markerTilemap.cellBounds.allPositionsWithin)
        {
            if (markerTilemap.HasTile(cell))
            {
                markerCells.Add(cell);
            }
        }

        if (markerCells.Count == 0)
        {
            return levelPositions;
        }

        markerCells.Sort((a, b) =>
        {
            int xCompare = a.x.CompareTo(b.x);
            if (xCompare != 0)
                return xCompare;

            return a.y.CompareTo(b.y);
        });

        for (int i = 0; i < markerCells.Count; i++)
        {
            Vector3 worldPosition = markerTilemap.GetCellCenterWorld(markerCells[i]);
            levelPositions[i] = worldPosition;
        }

        return levelPositions;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (markerTilemap == null)
            return;

        List<Vector3Int> markerCells = new List<Vector3Int>();

        foreach (var cell in markerTilemap.cellBounds.allPositionsWithin)
        {
            if (markerTilemap.HasTile(cell))
            {
                markerCells.Add(cell);
            }
        }

        if (markerCells.Count == 0)
            return;

        markerCells.Sort((a, b) =>
        {
            int xCompare = a.x.CompareTo(b.x);
            if (xCompare != 0)
                return xCompare;

            return a.y.CompareTo(b.y);
        });

        for (int i = 0; i < markerCells.Count; i++)
        {
            Vector3 worldPosition = markerTilemap.GetCellCenterWorld(markerCells[i]);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(worldPosition, 0.3f);

            Handles.Label(
                worldPosition + Vector3.up * 0.4f,
                $"Index {i}"
            );
        }
    }
#endif
}
