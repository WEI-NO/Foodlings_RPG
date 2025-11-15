using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MoneyUpgradeButton : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;
    public Color AffordableColor = Color.white;
    public Color UnaffordableColor = Color.red;

    private async void Start()
    {
        while (GameMatchManager.Instance == null || !GameMatchManager.Instance.started)
        {
            await Task.Yield();
        }

        UpdateDisplayInfo();
    }

    private void Update()
    {
        var ps = GameMatchManager.Instance;
        
        if (ps.HasMoney(ps.CostToUpgrade()))
        {
            costText.color = AffordableColor;
        } else
        {
            costText.color = UnaffordableColor;
        }
    }

    public void UpgradeFunction()
    {
        var ps = GameMatchManager.Instance;

        if (ps.UpgradeMoney())
        {
            UpdateDisplayInfo();
        }
    }

    void UpdateDisplayInfo()
    {
        var playerStats = GameMatchManager.Instance;
        if (playerStats == null) return;

        var cost = playerStats.CostToUpgrade();
        costText.text = $"{(cost == -1 ? "MAX" : $"${cost}")}";
        levelText.text = $"Level\n{playerStats.MoneyLevel}";
    }
}
