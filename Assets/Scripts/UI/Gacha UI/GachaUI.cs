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
        PerformPull(GachaType.Basic, 10);
    }

    public void PullPremium_Single()
    {
        PerformPull(GachaType.Premium, 1);
    }

    public void PullPremium_Ten()
    {
        PerformPull(GachaType.Premium, 10);
    }

    private void PerformPull(GachaType type, int amount)
    {
        if (GachaSystem.Instance.RollCharacters(type, amount, out var datas))
        {
            foreach (var character in datas)
            {
                PlayerCollection.Instance.AddCharacter(character);
            }

            obtainUI.StartView(datas);
        }
    }
}
