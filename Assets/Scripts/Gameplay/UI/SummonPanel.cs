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

    public List<CharacterData> overrideSummonCharacter = new();
    public List<int> overrideSummonCharacterLevel = new();

    private List<CharacterInstance> temporaryCharacterInstance = new();

    public bool overrideCharacters = false;
    private bool hasPartyMembers = false;

    public bool summonPanelReady = false;

    private async void Start()
    {
        party = PlayerParty.Instance;
        summonPanelReady = false;

        while (!GameBoostrapper.Instance.DataLoaded)
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
            hasPartyMembers = true;
        }
        summonPanelReady = true;

    }

    private void Update()
    {
        if (!summonPanelReady) return;
        if (overrideCharacters && !hasPartyMembers)
        {
            for (int i = summonButtons.Count - 1; i >= 0; i--)
            {
                summonButtons[i].AssignUnit(null);
            }

            temporaryCharacterInstance.Clear();

            for (int i = 0; i < overrideSummonCharacter.Count; i++)
            {
                CharacterInstance instance = new CharacterInstance();
                instance.ResetCharacter(overrideSummonCharacter[i]);
                instance.SetLevel(overrideSummonCharacterLevel[i]);
                temporaryCharacterInstance.Add(instance);
                summonButtons[i].AssignUnit(instance);
                hasPartyMembers = true;
            }


            overrideCharacters = false;
        }
    }
}
