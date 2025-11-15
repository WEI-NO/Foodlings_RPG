using System;
using UnityEngine;

public class TeamPedestal : MonoBehaviour
{
    [Header("Components")]
    public CharacterPedestal[] pedestals = new CharacterPedestal[5];

    private void Awake()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < transform.childCount)
            {
                pedestals[i] = transform.GetChild(i).GetComponent<CharacterPedestal>();
                pedestals[i].partyIndex = i;
            }
        }
        UpdateAll();
    }

    private void Start()
    {
        PlayerParty.Instance.OnPartyChange += OnPartyMemberChange;

    }

    private void OnCharacterRemoved(CharacterInstance instance)
    {
        foreach (var p in pedestals)
        {
            if (p.currentData == null) continue;
            if (p.currentData.instanceId == instance.instanceId)
            {
                p.SetCharacter(null);
            }
        }
    }

    private void OnEnable()
    {
        UpdateAll();
    }

    public void UpdateAll()
    {
        for (int i = 0; i < pedestals.Length; i++)
        {
            if (pedestals[i] == null || PlayerParty.Instance == null) continue;
            var data = PlayerParty.Instance.GetData(i);

            pedestals[i].SetCharacter(data);
        }
    }

    // Null is acceptable as "data"
    public void OnPartyMemberChange(CharacterInstance data, int index)
    {
        var p = pedestals[index];
        if (p == null) return;

        p.SetCharacter(data);
    }

}
