using CustomLibrary.References;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollection : MonoBehaviour
{
    public static PlayerCollection Instance;

    // Fast lookup by ID
    private readonly Dictionary<string, CharacterInstance> CollectionByID = new();
    // Display/order list
    private readonly List<string> CollectionByOrder = new();

    [Header("Callbacks")]
    public Action<CharacterInstance> OnCharacterAdded;
    public Action<CharacterInstance> OnCharacterRemoved;
    public Action<CharacterInstance, int> OnAddedToParty;
    public Action<CharacterInstance, int> OnRemovedFromParty;

    [Header("Debug Settings")]
    public List<CharacterData> debug_InitialCollection = new();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        // Debug Test
        foreach (var i in debug_InitialCollection)
        {
            if (i == null) continue;

            AddCharacter(i);
        }
    }

    public void AddCharacter(CharacterData data)
    {
        CharacterInstance newInstance = new CharacterInstance();
        if (!newInstance.ResetCharacter(data))
        {
            return;
        }

        // Storage
        CollectionByID.Add(newInstance.instanceId, newInstance);
        CollectionByOrder.Add(newInstance.instanceId);

        // Callback
        OnCharacterAdded?.Invoke(newInstance);
    }

    public void RemoveCharacter(string instanceID)
    {
        if (CollectionByID.TryGetValue(instanceID, out var instance))
        {
            OnCharacterRemoved?.Invoke(instance);
            CollectionByID.Remove(instanceID);
            CollectionByOrder.Remove(instanceID);
        }
    }

    public List<CharacterInstance> GetSorted(CollectionSortMode mode)
    {
        // Build the raw list
        List<CharacterInstance> result = new();
        foreach (var id in CollectionByOrder) // preserves acquisition order
        {
            if (CollectionByID.TryGetValue(id, out var inst))
                result.Add(inst);
        }

        switch (mode)
        {
            case CollectionSortMode.Level:
                // Highest level first, fallback to acquired order
                result.Sort((a, b) => b.level.CompareTo(a.level));
                break;

            case CollectionSortMode.Date:
                // Already in acquired order because we used CollectionByOrder
                // So no sorting needed
                break;
        }

        return result;
    }

    public bool AssignTeam(CharacterInstance instance, int index)
    {
        var id = instance.instanceId;
        if (CollectionByID.TryGetValue(id, out var inst))
        {
            inst.inPartyIndex = index;
            OnAddedToParty?.Invoke(inst, index);
            return true;
        }
        return false;
    }

    public bool UnassignTeam(CharacterInstance instance, out int lastIndex)
    {
        lastIndex = 0;
        var id = instance.instanceId;
        if (CollectionByID.TryGetValue(id, out var inst))
        {
            lastIndex = inst.inPartyIndex;
            inst.inPartyIndex = -1;
            OnRemovedFromParty?.Invoke(inst, lastIndex);
            return true;
        }
        return false;
    }

    public CharacterInstance FindData(string id)
    {
        if (CollectionByID.TryGetValue(id, out var data))
        {
            return data;
        }

        return null;
    }
}
