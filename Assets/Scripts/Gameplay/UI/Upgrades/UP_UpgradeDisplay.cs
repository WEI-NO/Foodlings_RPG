using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UP_UpgradeDisplay : MonoBehaviour
{
    [Header("References")]
    UpgradeDef assignedDef;

    [Header("Components")]
    public Image icon;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;


    public void AssignUpgradeDef(UpgradeDef upgrade)
    {
        assignedDef = upgrade; 
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (assignedDef == null)
        {
            icon.gameObject.SetActive(false);
            titleText.text = "???";
            descriptionText.text = "???";
            costText.text = "Cost: ???";
            levelText.text = "Level ???";
        } else
        {
            icon.gameObject.SetActive(true);
            titleText.text = $"{assignedDef.Title}";
            descriptionText.text = $"{assignedDef.Description}";

            var level = PlayerStats.Instance.GetCurrentLevel(assignedDef.Id);

            costText.text = $"Cost: {assignedDef.CostForLevel(level)}";
            levelText.text = $"Level {level}";
        }
    }

    public void UpgradeAction()
    {
        var level = PlayerStats.Instance.GetCurrentLevel(assignedDef.Id);
        int cost = assignedDef.CostForLevel(level);

        if (PlayerInventory.UseItem("toast_coin", cost))
        {
            PlayerStats.Instance.Upgrade(assignedDef.Id);
            UpdateDisplay();
        }
    }
}
