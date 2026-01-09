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

    public static int MaxRoleStage = 5;

    public int CostPerStage = 50;

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

        return new Vector3Int(f * CostPerStage, t * CostPerStage, m * CostPerStage);
    }

    public bool AttemptCraft(GachaRequest request)
    {
        var costs = GetCraftCost(request);

        if (PlayerInventory.HasItem("seared_core", costs.x) && PlayerInventory.HasItem("crusted_core", costs.y) && PlayerInventory.HasItem("infused_core", costs.z))
        {
            PlayerInventory.UseItem("seared_core", costs.x);
            PlayerInventory.UseItem("crusted_core", costs.y);
            PlayerInventory.UseItem("infused_core", costs.z);
            return true;
        }

        return false;
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

    private int Clamp(int value)
    {
        return Mathf.Clamp(value, 1, GachaSystem.MaxRoleStage);
    }
}