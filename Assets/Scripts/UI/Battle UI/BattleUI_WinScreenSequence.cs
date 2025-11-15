using CustomLibrary.References;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleUI_WinScreenSequence : MonoBehaviour
{
    public static BattleUI_WinScreenSequence Instance;

    [Header("Components")]
    public BattleUI_ItemRewardPanel rewardPanel;
    // Info
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI timePassed;

    [Header("Settings")]
    public float initialSpawnDelay;

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        rewardPanel = BattleUI_ItemRewardPanel.Instance;
    }


    public void StartSequence(List<RewardEntry> rewards)
    {
        StartCoroutine(Sequence(rewards));
    }

    private IEnumerator Sequence(List<RewardEntry> rewards)
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        yield return BattleUI_ItemRewardPanel.Instance.ShowRewards(rewards);
    }
}
