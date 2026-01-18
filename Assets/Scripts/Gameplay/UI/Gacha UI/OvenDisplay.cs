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
    public GameObject claimButton;
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
        ToggleInProgress(ShouldDisplayProgress());
        oven.OnOvenEnd += OnOvenEnd;
        oven.OnOvenStart += OnOvenStart;
    }

    private void Update()
    {
        if (oven != null && ShouldDisplayProgress())
        {
            if (ShouldDisplayProgress())
            {
                print($"Oven: {oven.currentSeconds}");
                if (inprogressTimerText)
                {
                    inprogressTimerText.text = $"{TimeFormatter.TimeToDisplay(Mathf.Clamp(oven.currentSeconds, 0, oven.currentSeconds))}";
                }
            }
        } 
    }

    private bool ShouldDisplayProgress()
    {
        return oven.StateIs(OvenState.Ready) || oven.StateIs(OvenState.InProgress);
    }

    private void OnOvenEnd()
    {
        if (claimButton)
        {
            claimButton.SetActive(true);
        }
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

        if (claimButton)
        {
            claimButton.SetActive(oven.StateIs(OvenState.Ready));
        }
    }

    public void OnClaim()
    {
        ToggleInProgress(false);
        oven.Claim();
    }
}
