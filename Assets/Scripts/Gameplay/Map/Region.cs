using UnityEngine;
using UnityEngine.Tilemaps;

public class Region : MonoBehaviour
{
    public Tilemap mainTilemap;
    public Tilemap effectTilemap;
    public Vector2Int overworldLocation;

    public Transform levelContainer;
    public Level assignedLevel;

    private void Initialize(int id)
    {
        foreach (Transform t in levelContainer)
        {
            t.GetComponent<OverworldLevelController>().regionIndex = id;
        }
    }
}
