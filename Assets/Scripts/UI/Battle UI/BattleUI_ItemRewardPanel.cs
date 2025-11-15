using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using CustomLibrary.References;

public class BattleUI_ItemRewardPanel : MonoBehaviour
{
    public static BattleUI_ItemRewardPanel Instance;

    [Header("Components")]
    public Transform content;
    public BattleUI_RewardItemDisplay displayPrefab;

    [Header("Settings")]
    public float spawnInterval;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    public IEnumerator ShowRewards(List<RewardEntry> rewards)
    {
        foreach (var r in rewards)
        {
            var rdata = MainDatabase.Instance.itemDatabase.Get(r.rewardID);
            var newDisplay = Instantiate(displayPrefab, content);
            newDisplay.ShowReward(rdata, r.GetQuantity());
            yield return new WaitForSeconds(spawnInterval);
        }
    }
    
}
