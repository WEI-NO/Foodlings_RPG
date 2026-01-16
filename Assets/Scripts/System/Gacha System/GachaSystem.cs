using CustomLibrary.References;
using JetBrains.Annotations;
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

    public static int MaxRoleStage = 5;
    public static string requiredPrimaryResource = "toast_coin";

    public readonly int[] CostPerStage = new int[6] { 0, 2, 4, 6, 8, 10 };
    public readonly int[] PrimaryResPerStage = new int[6] { 0, 200, 500, 900, 1400, 1600 };

    public StarRarityTable rarityTable;
    public RoleBiasSettings roleBiasSettings;


    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    public Vector3Int GetCraftCost(GachaRequest request)
    {
        if (request == null) return new Vector3Int();

        int f = request.GetStage(Role.Fighter);
        int t = request.GetStage(Role.Tank);
        int m = request.GetStage(Role.Magic);

        return new Vector3Int(CostPerStage[f], CostPerStage[t], CostPerStage[m]);
    }

    public int GetCraftCost(GachaRequest request, Role role)
    {
        if (request == null) return 0;

        int cost = request.GetStage(role);

        return CostPerStage[cost];
    }


    public bool AttemptCraft(GachaRequest request)
    {
        var costs = GetCraftCost(request);

        var primaryRes = PrimaryResCost(request);

        if (!PlayerInventory.HasItem(requiredPrimaryResource, primaryRes))
        {
            return false;
        }

        if (PlayerInventory.HasItem("seared_core", costs.x) && PlayerInventory.HasItem("crusted_core", costs.y) && PlayerInventory.HasItem("infused_core", costs.z))
        {
            PlayerInventory.UseItem(requiredPrimaryResource, primaryRes);
            PlayerInventory.UseItem("seared_core", costs.x);
            PlayerInventory.UseItem("crusted_core", costs.y);
            PlayerInventory.UseItem("infused_core", costs.z);
            return true;
        }

        return false;
    }

    public int PrimaryResCost(GachaRequest request)
    {
        int cost = PrimaryResPerStage[request.stages[0]] + PrimaryResPerStage[request.stages[1]] + PrimaryResPerStage[request.stages[2]];

        return cost;
    }

}

public class GachaRequest
{
    public int[] stages = new int[3];

    public GachaRequest(int seared, int crusted, int infused)
    {
        stages[0] = Clamp(seared);
        stages[1] = Clamp(crusted);
        stages[2] = Clamp(infused);
    }

    public int GetStage(Role role)
    {
        if (role == Role.Support) return 0;

        return stages[role.ToInt()];
    }

    public void ChangeStages(Role role, int amount)
    {
        if (role == Role.Support)
        {
            return;
        }

        int i = role.ToInt();

        stages[i] = Clamp(stages[i] + amount);

    }
    
    public int Total()
    {
        int total = 0;
        foreach (var s in stages)
        {
            total += s;
        }
        return total;
    }

    public bool Valid()
    {
        return Total() > 0;
    }

    private int Clamp(int value)
    {
        return Mathf.Clamp(value, 0, GachaSystem.MaxRoleStage);
    }

}