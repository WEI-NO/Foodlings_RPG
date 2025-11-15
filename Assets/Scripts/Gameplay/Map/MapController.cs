using CustomLibrary.References;
using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController Instance;

    [Header("Map Properties")]
    [SerializeField] private float mapLength = 10.0f; // With respect to the middle (0)
    // Map Bounds
    public float GetBound_Left { get { return 0 - mapLength / 2.0f; } private set { } }
    public float GetBound_Right { get { return 0 + mapLength / 2.0f; } private set { } }

    [Header("Gameplay Objects")]
    [SerializeField] private Tower PlayerBase;
    public Tower spawnedPlayerBase;
    [SerializeField] private Tower EnemyBase;
    public Tower spawnedEnemyBase;

    [Header("Visual Objects")]
    [SerializeField] private GameObject walkingPath;
    [SerializeField] private GameObject foregroundGrass;
    [SerializeField] private Transform pathsContainer;
    [SerializeField] private Transform foregroundContainer;

    [Header("Visual Settings")]
    public float groundGap = 12.0f;
    public float groundYPosition = -2f;
    public float foregroundYPosition = -8f;
    public int extraGroundPerSide = 2;

    [Header("Spawning Settings")]
    [SerializeField] private float spawnY;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        StartGameSequence();
    }

    public void StartGameSequence()
    {
        SetupVisual();

        SetupBases();

        if (spawnedPlayerBase == null) { Debug.LogWarning("PlayerBase was not spawned correctly."); return; }
        if (spawnedEnemyBase == null) { Debug.LogWarning("EnemyBase was not spawned correctly."); return; }


    }

    private void SetupVisual()
    {
        // Clear all children
        for (int i = pathsContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(pathsContainer.GetChild(i).gameObject);
        }
        for (int i = foregroundContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(foregroundContainer.GetChild(i).gameObject);
        }

        int requiredSegments = Mathf.CeilToInt(mapLength/ groundGap);
        if (requiredSegments % 2 == 0) requiredSegments++; // Will always be odd number
        requiredSegments += 2 * extraGroundPerSide;

        Vector2 pos = Vector2.zero;

        for (int i = 0; i < requiredSegments; i++)
        {
            Transform ground = Instantiate(walkingPath).transform;
            Transform foreground = Instantiate(foregroundGrass).transform;
            if (i == 0) pos = new Vector2(0, groundYPosition);
            else
            {
                int side = (i % 2 == 1) ? 1 : -1; // Odd = 1 (Right Side), Even = -1 (Left Side)
                int step = i / 2; // 1, 1, 2, 2, 3, 3, etc. 
                pos = new Vector2(step * groundGap * side, groundYPosition);
            }

            ground.position = pos;
            ground.SetParent(pathsContainer);
            foreground.position = new Vector2(pos.x, foregroundYPosition);
            foreground.SetParent(foregroundContainer);
        }

    }

    private void SetupBases()
    {
        if (PlayerBase)
        {
            spawnedPlayerBase = Instantiate(PlayerBase, transform, false);
            spawnedPlayerBase.transform.position = new Vector2(GetBound_Left, spawnY);
        }

        if (EnemyBase)
        {
            spawnedEnemyBase = Instantiate(EnemyBase, transform, false);
            spawnedEnemyBase.transform.position = new Vector2(GetBound_Right, spawnY);
        }
    }

}
