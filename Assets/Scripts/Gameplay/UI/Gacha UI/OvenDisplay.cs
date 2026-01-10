using CustomLibrary.References;
using UnityEngine;

public class OvenDisplay : MonoBehaviour
{
    public int OvenIndex;

    public BannerController[] banners = new BannerController[3];

    public Oven oven;

    private void OnDestroy()
    {
        if (oven != null)
            oven.OnOvenEnd -= OnOvenEnd;
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

        oven.OnOvenEnd += OnOvenEnd;
    }

    private void Update()
    {
        if (oven != null && oven.StateIs(OvenState.InProgress))
        {
            print($"Oven: {oven.currentSeconds}");
        }
    }

    private void OnOvenEnd()
    {
        print($"Rolled |  Star: {oven.savedRanked} Role: {oven.savedRole}");
    }

    public void IncrementRequest(BannerController controller)
    {
        oven.request.ChangeStages(controller.coreRole, 1);
        foreach (var b in banners)
        {
            if (b == null) continue;
            b.UpdateDisplay();
        }
    }

    public void DecrementRequest(BannerController controller)
    {
        oven.request.ChangeStages(controller.coreRole, -1);
        foreach (var b in banners)
        {
            if (b == null) continue;
            b.UpdateDisplay();
        }
    }

    public void Cook()
    {
        oven.StartOven();
    }

}
