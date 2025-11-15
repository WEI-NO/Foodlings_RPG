using UnityEngine;
using CustomLibrary.References;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class GameMatchManager : MonoBehaviour
{
    public static GameMatchManager Instance;

    public static Level CurrentSelectedLevel;

    public Action OnFriendlyWin;
    public Action OnEnemyWin;

    public int gamePauseLock = 0;
    public bool gameOngoing = false;
    public static bool UnlockUponCompletion = false;
    public static List<int> UnlockRegion = new();

    [Header("Game Information")]
    public float gameTimer = 0.0f;
    public int killCount = 0;

    public bool started = false;

    [Header("Round Money Upgrades")]
    public int MaxMoneyLevel = 7;
    public int MoneyLevel = 1;

    [Header("Round Money Cap Settings")]
    //public int BaseMoneyCap; // The baseline for max money
    public float MoneyCapCompoundPercentage = 0.3f; // Compound of the currentMaxMoney 0.3f = +30%
    public int CurrentMoneyCap;

    [Header("Round Money Speed Settings")]
    public float BaseMoneySpeed; // Money / s
    public float MoneySpeedCompoundPercentage = 0.15f;
    public float CurrentMoneySpeed;
    private float moneySpeedCounter = 0.0f;

    [Header("Round Money")]
    public int RoundMoney = 0;

    [Header("Upgrade Cost Settings")]
    public float kMultiplier;

    public static void SetLevel(Level level, bool unlockUponCompletion = false, List<int> unlockRegion = null)
    {
        if (unlockUponCompletion)
        {
            UnlockUponCompletion = true;
            UnlockRegion = unlockRegion;
        } else
        {
            UnlockRegion = null;
        }
        CurrentSelectedLevel = level;
    }

    public Level currentLevel;

    private async void Awake()
    {
        Initializer.SetInstance(this);

        started = false;


        while (CharacterDatabase.Instance == null || !CharacterDatabase.Instance.IsReady || Instance.GamePaused())
        {
            await Task.Yield();
        }

        while (PlayerStats.Instance == null)
        {
            await Task.Yield();
        }

        ResetPlayerStats();


        SceneTransitor.OnSceneTransitionCompleted += (n) =>
        {
            if (n == "Game Scene")
            {
                ResetPlayerStats();
            }
        };
        started = true;
    }

    private void Start()
    {
        if (CurrentSelectedLevel)
        {
            currentLevel = Instantiate(CurrentSelectedLevel);
            if (currentLevel)
                currentLevel.Initialize();
        }
        killCount = 0;
        gameTimer = 0.0f;
    }

    private void Update()
    {
        if (currentLevel && !GamePaused())
        {
            gameTimer += Time.deltaTime;
            currentLevel.UpdateLevel(Time.deltaTime);
        }

        if (!started || Instance.GamePaused()) return;


        if (RoundMoney >= CurrentMoneyCap)
        {
            return;
        }
        moneySpeedCounter += Time.deltaTime;
        float interval = 1.0f / CurrentMoneySpeed;

        int numTicks = Mathf.FloorToInt(moneySpeedCounter / interval);
        if (numTicks > 0)
        {
            RoundMoney += numTicks;
            moneySpeedCounter -= numTicks * interval;
        }
    }

    public void SetGameResult(bool result) // True = player win, False = Enemy win
    {
        if (result)
        {
            OnFriendlyWin?.Invoke();

            // Rewards
            if (currentLevel)
            {
                var rewards = currentLevel.clearRewards;
                // First Clear
                if (!OverworldData.Instance.CompletedLevel(currentLevel))
                {
                    rewards = currentLevel.firstClearRewards;
                }

                GiveLevelRewards(rewards);
            }

            OverworldData.Instance.CompleteLevel(UnlockUponCompletion, UnlockRegion);
        } else
        {
            OnEnemyWin?.Invoke();
        }
        SetGameState(true);
    }

    public void SetGameState(bool state)
    {
        gamePauseLock += state ? -1 : 1;
        Mathf.Clamp(gamePauseLock, 0, int.MaxValue);
    }

    public bool GamePaused()
    {
        return gamePauseLock > 0;
    }

    public void GiveLevelRewards(List<RewardEntry> rewards)
    {
        foreach (var r in rewards)
        {
            PlayerInventory.AddItem(r.rewardID, r.GetQuantity());
        }
        BattleUI_WinScreenSequence.Instance.StartSequence(rewards);
    }

    #region Cost

    public bool HasMoney(int amount)
    {
        return RoundMoney >= amount;
    }

    public bool UseMoney(int amount)
    {
        if (HasMoney(amount))
        {
            RoundMoney -= amount;
            return true;
        }
        return false;
    }

    public int CostToUpgrade()
    {
        if (MoneyLevel >= MaxMoneyLevel)
        {
            return -1;
        }

        var cost = Mathf.RoundToInt(kMultiplier * CurrentMoneyCap);
        return cost;
    }

    #endregion cost

    private void ResetPlayerStats()
    {
        CurrentMoneyCap = (int)PlayerStats.Instance.GetValue("maxmoneycap");
        CurrentMoneySpeed = BaseMoneySpeed;
        RoundMoney = 0;
        MoneyLevel = 1;
    }

    public bool UpgradeMoney()
    {
        if (MoneyLevel >= MaxMoneyLevel) return false;

        if (!UseMoney(CostToUpgrade()))
        {
            return false;
        }

        CurrentMoneyCap += (int)(CurrentMoneyCap * MoneyCapCompoundPercentage);
        CurrentMoneySpeed += CurrentMoneySpeed * MoneySpeedCompoundPercentage;
        MoneyLevel++;
        return true;
    }
}
