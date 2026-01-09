using CustomLibrary.References;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CollectionSortMode
{
    Level,
    Date
}


public class CollectionDisplay : MonoBehaviour
{
    public static CollectionDisplay Instance;

    public Transform content;
    public CharacterDisplay displayPrefab;
    public Dictionary<string, CharacterDisplay> displays = new();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void OnEnable()
    {
        if (PlayerCollection.Instance == null) return;

        RefreshDisplay();
    }

    private void Start()
    {
        RefreshDisplay();

        PlayerCollection.Instance.OnCharacterAdded += OnCharacterAdded;
        PlayerCollection.Instance.OnCharacterRemoved += OnCharacterRemoved;
        PlayerCollection.Instance.OnAddedToParty += AddToTeam;
        PlayerCollection.Instance.OnRemovedFromParty += RemoveFromTeam;
    }

    private void ClearDisplay()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        displays.Clear();
    }

    private void RefreshDisplay(CollectionSortMode sortMode = CollectionSortMode.Date)
    {
        ClearDisplay();

        var sortedList = PlayerCollection.Instance.GetSorted(sortMode);

        // 1. Equipped characters (0–4), ordered by party slot
        var equipped = sortedList
            .Where(c => c.inPartyIndex >= 0)
            .OrderBy(c => c.inPartyIndex);

        // 2. Unequipped characters
        var unequipped = sortedList
            .Where(c => c.inPartyIndex < 0);

        // 3. Display equipped first
        foreach (var character in equipped)
        {
            OnCharacterAdded(character);
        }

        // 4. Then display the rest
        foreach (var character in unequipped)
        {
            OnCharacterAdded(character);
        }
    }

    private void OnCharacterAdded(CharacterInstance instance)
    {
        if (instance == null) return;

        var newDisplay = Instantiate(displayPrefab, content);
        newDisplay.AssignInstance(instance);

        displays.Add(instance.instanceId, newDisplay);
    }

    private void OnCharacterRemoved(CharacterInstance instance)
    {
        if (instance == null) return;

        if (displays.TryGetValue(instance.instanceId, out var display))
        {
            if (display != null)
                Destroy(display.gameObject);
            displays.Remove(instance.instanceId);
        }
    }

    public void AddToTeam(CharacterInstance instance, int index)
    {
        var id = instance.instanceId;
        if (displays.TryGetValue(id, out var display))
        {
            // WIP;
            // Turn on in team visual
            display.UpdateEquipSign();
        }
    }

    public void RemoveFromTeam(CharacterInstance instance, int index)
    {
        var id = instance.instanceId;
        if (displays.TryGetValue(id, out var display))
        {
            // WIP;
            // Turn off in team
            display.UpdateEquipSign();
        }
    }
}
