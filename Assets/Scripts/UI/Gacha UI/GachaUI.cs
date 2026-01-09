using UnityEngine;

public class GachaUI : BaseUIPage
{
    
    public CharacterObtainUIPage obtainUI;

    public void PullBasic_Single()
    {
        PerformPull(GachaType.Basic, 1);
    }

    public void PullBasic_Ten()
    {
        PerformPull(GachaType.Basic, 5);
    }

    public void PullPremium_Single()
    {
        PerformPull(GachaType.Premium, 1);
    }

    public void PullPremium_Ten()
    {
        PerformPull(GachaType.Premium, 5);
    }

    private void PerformPull(GachaType type, int amount)
    {

    }
}
