using UnityEngine;
using TMPro;

public class MoneyDisplay : MonoBehaviour
{
    public TextMeshProUGUI moneyDisplayText;

    private void Update()
    {
        if (GameMatchManager.Instance == null) return;

        string text = $"{GameMatchManager.Instance.RoundMoney} / {GameMatchManager.Instance.CurrentMoneyCap}";
        moneyDisplayText.text = text;
    }

}
