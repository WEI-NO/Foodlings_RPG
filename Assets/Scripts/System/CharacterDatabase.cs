using CustomLibrary.References;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CharacterDatabase : MonoBehaviour
{
    public static CharacterDatabase Instance;

    // Runtime indexes
    private readonly Dictionary<string, CharacterData> byId = new();
    private readonly Dictionary<UnitRarity, List<CharacterData>> byRarity = new();
    private readonly Dictionary<Faction, List<CharacterData>> byFaction = new();

    [Header("Addressables")]
    [SerializeField] private string unitsLabel = "Characters"; // label applied to all CharacterData assets

    public bool IsReady { get; private set; }

    private async void Awake()
    {
        IsReady = false;
        Initializer.SetInstance(this); 
        DontDestroyOnLoad(gameObject);
        // (Optional but recommended in builds)
        await Addressables.InitializeAsync().Task;

        try
        {
            await InitializeAsync();
            Debug.Log("CharacterDatabase ready.");
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
    public async Task InitializeAsync()
    {
        if (IsReady) return;

        // Load all UnitData assets with a shared label (no Resources folder needed)
        AsyncOperationHandle<IList<CharacterData>> handle =
            Addressables.LoadAssetsAsync<CharacterData>(unitsLabel, null);

        var allUnits = await handle.Task;

        // Build indexes
        byId.Clear();
        byRarity.Clear();
        byFaction.Clear();

        foreach (var u in allUnits)
        {
            if (string.IsNullOrEmpty(u.id))
                Debug.LogWarning($"CharacterData missing id: {u.name}");

            byId[u.id] = u;

            if (!byRarity.TryGetValue(u.rarity, out var listR))
                byRarity[u.rarity] = listR = new List<CharacterData>();
            listR.Add(u);

            if (!byFaction.TryGetValue(u.faction, out var listF))
                byFaction[u.faction] = listF = new List<CharacterData>();
            listF.Add(u);
        }

        IsReady = true;
    }

    // O(1) lookup
    public CharacterData GetById(string id) => byId.TryGetValue(id, out var u) ? u : null;

    // Fast queries
    public IReadOnlyList<CharacterData> GetByRarity(UnitRarity r) =>
        byRarity.TryGetValue(r, out var list) ? list : System.Array.Empty<CharacterData>();

    public IReadOnlyList<CharacterData> GetByFaction(Faction f) =>
        byFaction.TryGetValue(f, out var list) ? list : System.Array.Empty<CharacterData>();
}
