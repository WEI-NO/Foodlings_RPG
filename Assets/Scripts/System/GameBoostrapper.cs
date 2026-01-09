using CustomLibrary.References;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameBoostrapper : MonoBehaviour
{
    public static GameBoostrapper Instance;

    [Header("Database Prefabs")]
    [SerializeField] private CharacterDatabase characterDatabasePrefab;
    [SerializeField] private MainDatabase mainDatabasePrefab;

    public bool DataLoaded { get; private set; }

    private async void Awake()
    {
        DataLoaded = false;

        Initializer.SetInstance(this);

        // 1. Initialize Addressables API
        await Addressables.InitializeAsync().Task;

        // 2. Initialize CharacterDatabase
        await characterDatabasePrefab.InitializeAsync();


        DataLoaded = true;
    }
}
