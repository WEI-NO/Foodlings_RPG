using UnityEngine;
using UnityEngine.Tilemaps;

public class Region : MonoBehaviour
{
    public Tilemap mainTilemap;
    public Tilemap effectTilemap;
    public Vector2Int overworldLocation;

    public Transform levelContainer;
}
