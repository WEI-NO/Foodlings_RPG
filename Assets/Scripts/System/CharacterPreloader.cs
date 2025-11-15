using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLibrary.References;

public class CharacterPreloader : MonoBehaviour
{
    public static CharacterPreloader Instance;
    private readonly Dictionary<string, GameObject> loadedPrefabs = new();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }
    public async Task PreloadUnitsAsync(IEnumerable<CharacterData> units)
    {
        foreach (var unit in units)
        {
            if (loadedPrefabs.ContainsKey(unit.id)) continue;
            var handle = Addressables.LoadAssetAsync<GameObject>(unit.unitPrefab);
            var prefab = await handle.Task;
            loadedPrefabs[unit.id] = prefab;
        }
    }

    public GameObject GetPrefab(string id)
    {
        loadedPrefabs.TryGetValue(id, out var prefab);
        return prefab;
    }

    public void UnloadAll()
    {
        foreach (var kv in loadedPrefabs)
            Addressables.Release(kv.Value);
        loadedPrefabs.Clear();
    }
}