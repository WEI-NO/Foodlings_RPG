using CustomLibrary.References;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGachaInfo : MonoBehaviour
{
    public static PlayerGachaInfo Instance;

    public static int InitialOvenSlots = 1;
    public static int MaximumOvenSlots = 5;

    public int currentOvenSlot = 1;

    public List<Oven> Ovens = new();

    private void Awake()
    {
        Initializer.SetInstance(this);
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        foreach (var o in Ovens)
        {
            o.Update();
        }
    }

    private void Initialize()
    {
        // Check Save first
        for (int i= 0; i < currentOvenSlot; i++)
        {
            Ovens.Add(new Oven());
        }
    }

    public Oven GetOven(int slot)
    {
        if (slot < 0 || slot >= Ovens.Count)
        {
            return null;
        }

        return Ovens[slot];
    }
}

public enum OvenState
{
    Idle,
    InProgress,
    Ready
}

public class Oven
{
    public GachaRequest request;

    public bool ovenStarted = false;

    public float currentSeconds = 0;

    public UnitRank savedRanked;
    public Role savedRole;

    public Action OnOvenStart;
    public Action OnOvenEnd;

    public OvenState state;

    public Oven()
    {
        request = new GachaRequest(0, 0, 0);
        SetState(OvenState.Idle);
    }

    public bool StartOven()
    {
        if (!request.Valid()) return false;

        if (!StateIs(OvenState.Idle))
        {
            return false;
        }

        if (GachaSystem.Instance.AttemptCraft(request))
        {
            savedRanked = GachaSystem.Instance.rarityTable.RollStar(request.Total());
            currentSeconds = GachaSystem.Instance.rarityTable.GetCookTime(request.Total());
            savedRole = GachaSystem.Instance.roleBiasSettings.Calculate(request);
            SetState(OvenState.InProgress);
            OnOvenStart?.Invoke();
            return true;
        }

        return false;
    }

    public bool Update()
    {
        if (!StateIs(OvenState.InProgress)) return false;

        currentSeconds -= Time.deltaTime;
        if (currentSeconds <= 0)
        {
            SetState(OvenState.Ready);
            SetState(OvenState.Idle); // WIP Placeholder
            OnOvenEnd?.Invoke();
            return true;
        }
        return false;
    }

    public bool StateIs(OvenState state)
    {
        return state == this.state;
    }

    private void SetState(OvenState state)
    {
        this.state = state;
    }

}
