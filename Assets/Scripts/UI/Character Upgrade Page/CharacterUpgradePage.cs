using CustomLibrary.References;
using CustomLibrary.SpriteExtra;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterUpgradePage : BaseUIPage
{
    public static CharacterUpgradePage Instance;

    [Header("Settings")]
    [SerializeField] private CharacterInstance displayedCharacter;
    [SerializeField] private float characterIconSize = 64;

    [Header("Components")]
    public Image characterIconImage;
    public TextMeshProUGUI expText;
    public Image expFillbar;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthPointText;
    public TextMeshProUGUI pAttackText;
    public TextMeshProUGUI mAttackText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI attackRangeText;
    public TextMeshProUGUI pDefenseText;
    public TextMeshProUGUI mDefenseText;
    public TextMeshProUGUI movementSpeedText;
    public TextMeshProUGUI summonCost;
    public TextMeshProUGUI summonCD;
    public GameObject equippedIcon;
    public TextMeshProUGUI fusionCostText;

    [Header("Fusion Settings")]
    public Transform fusionContent;
    public int fusionCost = 0;
    public FuzeCharacterDisplay fuzeDisplayPrefab;
    public List<FuzeCharacterDisplay> displays;
    public List<CharacterInstance> selectedFusingUnit = new();
    public Action<CharacterInstance, bool> OnFusionChange;

    [Header("Extra Settings")]
    public CharacterDisplayFunction[] allFunctions = null;

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
        allFunctions = GetComponentsInChildren<CharacterDisplayFunction>();
        UpdateFusionCost();
    }

    protected override void OnStart()
    {
        OnFusionChange += OnFusionSelectionChange;
    }

    protected override void OnContentEnabled()
    {
        foreach (var func in allFunctions)
        {
            func.active = false;
        }
        ClearFusionSelection();
    }

    public void InitializeCharacter(CharacterInstance character)
    {
        displayedCharacter = character;
        ShowCharacter();
        displayedCharacter.OnExpChange += (CharacterInstance i, int j) => { UpdateLevelText(i); };
    }

    private void ShowCharacter()
    {
        if (displayedCharacter == null)
        {
            nameText.text = "Invalid Character";
        }
        else
        {
            var data = displayedCharacter.baseData;
            characterIconImage.sprite = data.unitSprite;
            characterIconImage.rectTransform.sizeDelta = SpriteExtra.DynamicDimension(data.unitSprite, characterIconSize);

            nameText.text = data.displayName;
            healthPointText.text = $"{displayedCharacter.GetStat(CharacterStatType.HP)}";
            pAttackText.text = $"{displayedCharacter.GetStat(CharacterStatType.PAtk)}";
            mAttackText.text = $"{displayedCharacter.GetStat(CharacterStatType.MAtk)}";
            attackSpeedText.text = $"{displayedCharacter.GetStat(CharacterStatType.AtkSpe)}";
            attackRangeText.text = $"{displayedCharacter.GetStat(CharacterStatType.AtkRng)}";
            pDefenseText.text = $"{displayedCharacter.GetStat(CharacterStatType.PDef)}";
            mDefenseText.text = $"{displayedCharacter.GetStat(CharacterStatType.MDef)}";
            movementSpeedText.text = $"{displayedCharacter.GetStat(CharacterStatType.Spe)}";
            summonCost.text = $"{data.summonCost}";
            summonCD.text = $"{displayedCharacter.GetStat(CharacterStatType.CD)}";

            equippedIcon.SetActive(displayedCharacter.inPartyIndex != -1);

            UpdateLevelText(displayedCharacter);
        }

    }

    private void UpdateLevelText(CharacterInstance instance)
    {
        if (instance == null) return;

        levelText.text = $"Lvl. {instance.level}";
        if (expText != null) expText.text = $"{instance.exp}/{instance.GetExpRequired()}";
        if (expFillbar != null) expFillbar.fillAmount = (float)instance.exp / instance.GetExpRequired();
    }

    #region Fusion

    public void OnFusionSelectionChange(CharacterInstance instance, bool added)
    {
        bool notEmpty = selectedFusingUnit.Count > 0;

        if (instance == null) return;

        if (added)
        {
            var newDisplay = Instantiate(fuzeDisplayPrefab, fusionContent);
            newDisplay.Initialize(instance);
            displays.Add(newDisplay);
        }
        else
        {
            for (int i = displays.Count - 1; i >= 0; i--)
            {
                var dis = displays[i];
                if (dis.heldCharacter.instanceId == instance.instanceId)
                {
                    Destroy(displays[i].gameObject);
                    displays.RemoveAt(i);
                }
            }
        }

        UpdateFusionCost();
    }

    public bool ToggleFusion(CharacterInstance instance)
    {
        // Delete if it is selected again
        if (selectedFusingUnit.Contains(instance))
        {
            selectedFusingUnit.Remove(instance);
            OnFusionChange?.Invoke(instance, false);
            return false; // Already added
        }
        selectedFusingUnit.Add(instance);
        OnFusionChange?.Invoke(instance, true);
        return true;
    }

    private void ClearFusionSelection()
    {
        if (fusionContent == null) return;
        for (int i = fusionContent.childCount - 1; i > 0; i--)
        {
            var t = fusionContent.GetChild(i);
            if (t != null)
            {
                Destroy(t.gameObject);
            }
        }
        displays = new();
        selectedFusingUnit = new();
        UpdateFusionCost();
    }

    public bool IsDisplayedCharacter(CharacterInstance other)
    {
        if (other == null || displayedCharacter == null) return false;
        return other.instanceId == displayedCharacter.instanceId;
    }

    #endregion fusion

    #region Exp

    public void PerformFusion()
    {
        if (selectedFusingUnit.Count == 0) return;

        if (!PlayerInventory.UseItem("toast_coin", fusionCost))
        {
            return;
        }

        int exp = CalculatePendingExp();
        displayedCharacter.GiveExp(exp);

        foreach (var i in selectedFusingUnit)
        {
            PlayerCollection.Instance.RemoveCharacter(i.instanceId);
            OnFusionChange?.Invoke(i, false);
        }

        ShowCharacter();
        ClearFusionSelection();
    }

    private void UpdateFusionCost()
    {
        if (fusionCostText)
        {
            int totalCost = 0;
            foreach (var i in selectedFusingUnit)
            {
                totalCost += CharacterData.FusionCost(i);
            }
            fusionCostText.text = $"{totalCost}";
            fusionCost = totalCost;
        }
    }

    public int CalculatePendingExp()
    {
        int pendingExp = 0;

        foreach (var i in selectedFusingUnit)
        {
            pendingExp += Mathf.RoundToInt(CharacterInstance.defaultExpGiven * CharacterData.RankFusionScale[(int)i.rank]);
            pendingExp += Mathf.RoundToInt(i.accumulatedExp * 0.9f);
        }

        return pendingExp;
    }

    #endregion exp
}
