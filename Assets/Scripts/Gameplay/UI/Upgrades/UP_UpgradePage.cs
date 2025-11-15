using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UP_UpgradePage : BaseUIPage
{
    [Header("Components")]
    public Transform content;
    public List<UpgradeDef> upgrades;
    public UP_UpgradeDisplay displayPrefab;

    protected override void OnContentEnabled()
    {
        ClearDisplays();
        upgrades = MainDatabase.Instance.upgradeDatabase.GetAll();
        if (PlayerStats.Instance != null)
        {
            var playerUpgrades = PlayerStats.Instance.upgradeLevels;
            foreach (var u in upgrades)
            {
                var newDisplay = Instantiate(displayPrefab, content);
                newDisplay.AssignUpgradeDef(u);
            }
        }
    }

    protected override void OnContentDisabled()
    {
        base.OnContentDisabled();
    }

    private void ClearDisplays()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
