using CustomLibrary.References;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum GachaType
{
    Basic,
    Premium
}
public class GachaSystem : MonoBehaviour
{
    public static GachaSystem Instance;

    [Header("Gacha Pools")]
    public GachaPool basicPool;
    public GachaPool premiumPool;

    public string pulledCurrency;
    public int[] currencyUsage = new int[]
    {
        150,
        300
    };

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    public int GetCost(GachaType type, int amount)
    {
        return currencyUsage[(int)type] * amount;
    }

    public bool RollCharacters(GachaType type, int amount, out List<CharacterData> rolledDatas)
    {
        rolledDatas = new();

        int cost = GetCost(type, amount);
        if (PlayerInventory.HasItem("gem", cost))
        {
            for (int i = 0; i < amount; i++)
            {
                string rolled = type == GachaType.Basic ?
                    basicPool.RollCharacter() :
                    premiumPool.RollCharacter();

                var data = CharacterDatabase.Instance.GetById(rolled);
                rolledDatas.Add(data);
            }
        }

        return rolledDatas.Count > 0;
    }

}
