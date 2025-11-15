using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SummonPanel : MonoBehaviour
{
    const int MaxSummonCount = 5;

    PlayerParty party;

    [Header("Components")]
    [SerializeField] public List<SummonButton> summonButtons = new();

    private async void Start()
    {
        party = PlayerParty.Instance;

        while (!CharacterDatabase.Instance.IsReady)
            await Task.Yield();

        foreach (Transform b in transform)
        {
            summonButtons.Add(b.GetComponent<SummonButton>());
        }

        for (int i = 0; i < MaxSummonCount; i++)
        {
            if (i >= party.Party.Count)
            {
                summonButtons[i].AssignUnit(null);
                continue;
            }
            var data = PlayerCollection.Instance.FindData(party.Party[i]);
            summonButtons[i].AssignUnit(data);
        }

    }

    private void Update()
    {
        
    }
}
