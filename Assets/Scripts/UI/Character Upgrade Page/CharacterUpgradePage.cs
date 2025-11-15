using CustomLibrary.References;
using CustomLibrary.SpriteExtra;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Search;
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
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI attackSpeedText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI movementSpeedText;
    public GameObject equippedIcon;
    public TextMeshProUGUI fusionCostText;

    [Header("Fusion Settings")]
    public Transform fusionContent;
    public FuzeCharacterDisplay fuzeDisplayPrefab;
    public List<FuzeCharacterDisplay> displays;
    public GameObject fusionEmptyIndicator;
    public List<CharacterInstance> selectedFusingUnit = new();
    public Action<CharacterInstance, bool> OnFusionChange;

    protected override void OnAwake()
    {
        Initializer.SetInstance(this);
    }

    protected override void OnStart()
    {
        OnFusionChange += OnFusionSelectionChange;
    }

    protected override void OnContentEnabled()
    {
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
            healthPointText.text = $"{data.hp}";
            attackText.text = $"{data.damage}";
            attackSpeedText.text = $"{data.attackRate}";
            defenseText.text = $"{data.defense}";
            movementSpeedText.text = $"{data.moveSpeed}";

            equippedIcon.SetActive(displayedCharacter.inPartyIndex != -1);

            UpdateLevelText(displayedCharacter);
        }

    }

    private void UpdateLevelText(CharacterInstance instance)
    {
        if (instance == null) return;

        levelText.text = $"Lvl. {instance.level}";
        expText.text = $"{instance.exp}/{instance.GetExpRequired()}";
        expFillbar.fillAmount = (float)instance.exp / instance.GetExpRequired();
    }

    #region Fusion

    public void OnFusionSelectionChange(CharacterInstance instance, bool added)
    {
        bool notEmpty = selectedFusingUnit.Count > 0;
        fusionEmptyIndicator.SetActive(!notEmpty);

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
        for (int i = fusionContent.childCount - 1; i >= 0; i--)
        {
            var t = fusionContent.GetChild(i);
            if (t != null)
            {
                Destroy(t.gameObject);
            }
        }
        selectedFusingUnit = new();
        fusionEmptyIndicator.SetActive(false);
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

        int exp = CalculatePendingExp();
        displayedCharacter.GiveExp(exp);

        foreach (var i in selectedFusingUnit)
        {
            PlayerCollection.Instance.RemoveCharacter(i.instanceId);
        }

        ClearFusionSelection();
    }

    private void UpdateFusionCost()
    {
        fusionCostText.text = $"{CalculatePendingExp()}";
    }

    public int CalculatePendingExp()
    {
        int pendingExp = 0;

        foreach (var i in selectedFusingUnit)
        {
            pendingExp += CharacterInstance.defaultExpGiven;
            pendingExp += Mathf.RoundToInt(i.accumulatedExp * 0.9f);
        }

        return pendingExp;
    }

    #endregion exp
}
