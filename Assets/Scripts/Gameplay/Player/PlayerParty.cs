using CustomLibrary.References;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerParty : MonoBehaviour
{
    public static PlayerParty Instance;
    public static int MaxCharacterInParty = 5;

    public List<string> Party = new List<string>();
    public List<CharacterInstance> PartyInstance = new List<CharacterInstance>();
    public Task PreloadTask { get; private set; } = Task.CompletedTask;
    public Action<CharacterInstance, int> OnPartyChange;


    private void Awake()
    {
        Initializer.SetInstance(this);
        Party = new List<string>(5);
        for (int i = 0; i < 5; i++)
            Party.Add("");
    }


    async void Start()
    {
        await Task.Yield();

        int i = 0;
        foreach (var id in Party)
        {
            if (i >= MaxCharacterInParty) break;
            var data = PlayerCollection.Instance.FindData(id);
            if (data != null)
            {
                OnPartyChange?.Invoke(data, i);
            }
            i++;
        }
        PlayerCollection.Instance.OnCharacterRemoved += (CharacterInstance inst) => {
            if (inst.inPartyIndex != -1)
            {
                AddToParty(null, inst.inPartyIndex);
            }
        };
    }


    public CharacterInstance GetData(int index)
    {
        if (index < 0 || index >= Party.Count)
        {
            return null;
        }

        return PlayerCollection.Instance.FindData(Party[index]);
    }

    public bool HasTeamMembers()
    {
        foreach (var i in Party)
        {
            if (i != "")
            {
                return true;
            }
        }
        return false;
    }

    public void AddToParty(CharacterInstance instance, int index)
    {
        if (instance == null)
        {
            if (Party[index] != "")
            {
                PlayerCollection.Instance.UnassignTeam(PlayerCollection.Instance.FindData(Party[index]), out var lastIndex);
            }
            Party[index] = "";
            OnPartyChange?.Invoke(instance, index);
            return;
        }

        string id = instance.instanceId;
        bool switchedPedestal = false;
        // Changing the instance's display pedestal if it is already on team
        if (Party.Contains(id))
        {
            if (PlayerCollection.Instance.UnassignTeam(instance, out var lastIndex))
            {
                // Unequip instance if the slot has a current instance
                if (Party[index] != "" && instance.instanceId != Party[index])
                {
                    var i = PlayerCollection.Instance.FindData(Party[index]);
                    PlayerCollection.Instance.AssignTeam(i, lastIndex);
                    Party[lastIndex] = i.instanceId;
                    OnPartyChange?.Invoke(i, lastIndex);
                    switchedPedestal = true;
                } else
                {
                    Party[lastIndex] = "";
                    OnPartyChange?.Invoke(null, lastIndex);
                }
            }
        }

        if (PlayerCollection.Instance.AssignTeam(instance, index))
        {
            if (Party[index] != "" && instance.instanceId != Party[index] && !switchedPedestal)
            {
                PlayerCollection.Instance.UnassignTeam(PlayerCollection.Instance.FindData(Party[index]), out int lastind);
            }
            OnPartyChange?.Invoke(instance, index);
            Party[index] = id;
        }
        
    }
}
