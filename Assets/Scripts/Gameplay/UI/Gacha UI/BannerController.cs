using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BannerController : MonoBehaviour
{
    public Role coreRole;
    public string coreItemID;

    public OvenDisplay owner;

    [Header("UI Components")]
    public TextMeshProUGUI stageCountText;
    public TextMeshProUGUI materialRequireCountText;


    public void Initialize(OvenDisplay parent)
    {
        owner = parent;
        switch (coreRole)
        {
            case Role.Fighter:
                coreItemID = "seared_core";
                break;
            case Role.Tank:
                coreItemID = "crusted_core";
                break;
            case Role.Magic:
                coreItemID = "infused_core";
                break;
        }
    }

    public void UpdateDisplay()
    {
        if (owner == null) return;

        if (stageCountText)
        {
            stageCountText.text = $"{owner.oven.request.GetStage(coreRole)}";
        }

        if (materialRequireCountText)
        {
            materialRequireCountText.text = $"{GachaSystem.Instance.GetCraftCost(owner.oven.request, coreRole)}";
        }
    }

    public void IncreaseStage()
    {
        owner.IncrementRequest(this);
    }

    public void DecreaseStage()
    {
        owner.DecrementRequest(this);
    }
}
