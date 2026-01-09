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
    private readonly Dictionary<UnitRank, List<CharacterData>> byRarity = new();
    private readonly Dictionary<Role, List<CharacterData>> byRole = new();

    [Header("Addressables")]
    [SerializeField] private string unitsLabel = "Characters"; // label applied to all CharacterData assets

    public bool IsReady { get; private set; }

    private void Awake()
    {
        Initializer.SetInstance(this); 
    }
    public async Task InitializeAsync()
    {
        if (IsReady) return;

        // Load all UnitData assets with a shared label
        AsyncOperationHandle<IList<CharacterData>> handle =
            Addressables.LoadAssetsAsync<CharacterData>(unitsLabel, null);

        var allUnits = await handle.Task;

        // Build indexes
        byId.Clear();
        byRarity.Clear();
        byRole.Clear();

        foreach (var u in allUnits)
        {
            if (string.IsNullOrEmpty(u.id))
                Debug.LogWarning($"CharacterData missing id: {u.name}");

            byId[u.id] = u;

            if (!byRarity.TryGetValue(u.baseRank, out var listR))
                byRarity[u.baseRank] = listR = new List<CharacterData>();
            listR.Add(u);

            if (!byRole.TryGetValue(u.role, out var listF))
                byRole[u.role] = listF = new List<CharacterData>();
            listF.Add(u);
        }

        IsReady = true;
    }

    // O(1) lookup
    public CharacterData GetById(string id) => byId.TryGetValue(id, out var u) ? u : null;

    // Fast queries
    public IReadOnlyList<CharacterData> GetByRarity(UnitRank r) =>
        byRarity.TryGetValue(r, out var list) ? list : System.Array.Empty<CharacterData>();

    public IReadOnlyList<CharacterData> GetByRole(Role f) =>
        byRole.TryGetValue(f, out var list) ? list : System.Array.Empty<CharacterData>();
}
