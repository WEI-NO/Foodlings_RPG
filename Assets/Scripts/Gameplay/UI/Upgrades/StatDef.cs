using Unity.VisualScripting.FullSerializer;
using UnityEngine;


public enum StatType { MaxMoneyCap, MoneyIncomeRate, CharCooldown, CharCost }


[CreateAssetMenu(menuName = "Game/Stat")]
public class StatsDef : ScriptableObject
{
    [SerializeField] private string id;            // MUST be unique & never change
    public string Id => id;

    public string Title;
    [TextArea] public string Description;
    public StatType Type;

    public float baseValue;
    public bool useInteger = false;
    
    public float CalculateValue(UpgradeDef def, int level)
    {
        var currentValue = baseValue;
        if (def.Type == Type)
        {
            currentValue = baseValue + def.ValueAtLevel(level);
            if (useInteger)
            {
                currentValue = Mathf.Round(currentValue);
            }
        }
        return currentValue;
    }
}