using CustomLibrary.References;
using CustomLibrary.Time;
using TMPro;
using UnityEngine;

public class OvenDisplay : MonoBehaviour
{
    public int OvenIndex;

    public BannerController[] banners = new BannerController[3];

    public Oven oven;

    public TextMeshProUGUI primaryResRequireCountText;
    public Color[] enable_disable = new Color[2];

    public GameObject inprogressDisplay;
    public TextMeshProUGUI inprogressTimerText;

    private void OnDestroy()
    {
        if (oven != null)
        {
            oven.OnOvenEnd -= OnOvenEnd;
            oven.OnOvenStart -= OnOvenStart;
        }
    }

    public void Initialize(int index)
    {
        oven = PlayerGachaInfo.Instance.GetOven(index);

        OvenIndex = index;
        var b = GetComponentsInChildren<BannerController>();
        foreach (var b_0 in b)
        {
            int i = b_0.coreRole.ToInt();
            banners[i] = b_0;
            banners[i].Initialize(this);
            banners[i].UpdateDisplay();
        }

        UpdatePrimaryRes();
        ToggleInProgress(oven.StateIs(OvenState.InProgress));
        oven.OnOvenEnd += OnOvenEnd;
        oven.OnOvenStart += OnOvenStart;
    }

    private void Update()
    {
        if (oven != null && oven.StateIs(OvenState.InProgress))
        {
            if (oven.StateIs(OvenState.InProgress))
            {
                print($"Oven: {oven.currentSeconds}");
                if (inprogressTimerText)
                {
                    inprogressTimerText.text = $"{TimeFormatter.TimeToDisplay(oven.currentSeconds)}";
                }
            }
        } 
    }

    private void OnOvenEnd()
    {
        print($"Rolled |  Star: {oven.savedRanked} Role: {oven.savedRole}");
        ToggleInProgress(false);
    }

    private void OnOvenStart()
    {
        ToggleInProgress(true);
    }

    public void IncrementRequest(BannerController controller)
    {
        oven.request.ChangeStages(controller.coreRole, 1);
        foreach (var b in banners)
        {
            if (b == null) continue;
            b.UpdateDisplay();
        }
        UpdatePrimaryRes();
    }

    public void DecrementRequest(BannerController controller)
    {
        oven.request.ChangeStages(controller.coreRole, -1);
        foreach (var b in banners)
        {
            if (b == null) continue;
            b.UpdateDisplay();
        }
        UpdatePrimaryRes();
    }

    public void Cook()
    {
        oven.StartOven();
    }

    public void UpdatePrimaryRes()
    {
        if (!primaryResRequireCountText) return;

        primaryResRequireCountText.text = $"{GachaSystem.Instance.PrimaryResCost(oven.request)}";
        primaryResRequireCountText.color = PlayerInventory.HasItem(GachaSystem.requiredPrimaryResource, GachaSystem.Instance.PrimaryResCost(oven.request)) ? enable_disable[0] : enable_disable[1];
    }

    public void ToggleInProgress(bool state)
    {
        if (inprogressDisplay)
        {
            inprogressDisplay.SetActive(state);
        }
    }
}
