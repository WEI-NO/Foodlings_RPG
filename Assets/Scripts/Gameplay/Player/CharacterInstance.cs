using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class CharacterInstance
{
    // Unique per copy (lets you have duplicates of the same CharacterData)
    public string instanceId;                // e.g., Guid.NewGuid().ToString("N")
    public CharacterData baseData;           // reference to immutable template

    // Per-instance selected
    public int level = 1;
    public int exp = 0;
    public int accumulatedExp;
    public bool favorited = false;
    public int inPartyIndex = -1;
    public Action<CharacterInstance, int> OnLevelUp;
    public Action<CharacterInstance, int> OnExpChange;

    public UnitRank rank;

    public static int defaultExpGiven = 100;

    public bool ResetCharacter(CharacterData character)
    {
        if (character == null)
        {
            Debug.LogWarning($"{character.displayName} is not a valid character.");
            return false;
        }
        baseData = character;
        instanceId = Guid.NewGuid().ToString("N");
        level = 1;
        exp = 0;
        accumulatedExp = 0;
        favorited = false;
        inPartyIndex = -1;
        rank = character.baseRank;
        return true;
    }

    // Example equipment slots (replace with your own type/slots)
    public EquipmentInstance[] equipment = new EquipmentInstance[3];

    // ---- Derived stats (base * growth * gear, etc.) ----
    public float GetStat(CharacterStatType type)
    {
        float r = baseData.GetRawStat(type, level);
        switch (type)
        {
            case CharacterStatType.HP:
                r += EquipHP();
                break;
            case CharacterStatType.PAtk:
                r += EquipPDamage();
                break;
            case CharacterStatType.MAtk:
                r += EquipMDamage();
                break;
            case CharacterStatType.PDef:
                 r += EquipPDefense();
                break;
            case CharacterStatType.MDef:
                r += EquipMDefense();
                break;
            case CharacterStatType.AtkRng:
                r *= EquipRangeMul();
                break;
            case CharacterStatType.AtkSpe:
                r *= EquipAttackRateMul();
                break;
            case CharacterStatType.Spe:
                r *= EquipSpeedMul();
                break;
            case CharacterStatType.CD:
                r *= EquipCooldownReduction();
                break;
        }
        r = Mathf.Round(r * 10f) / 10f;
        return r;
    }

    // ---- Example gear hooks (stub out or integrate your system) ----
    private float EquipHP() => equipment.Where(e => e != null).Sum(e => e.FlatHP);
    private float EquipPDamage() => equipment.Where(e => e != null).Sum(e => e.FlatPAtk);
    private float EquipPDefense() => equipment.Where(e => e != null).Sum(e => e.FlatPDef);
    private float EquipMDamage() => equipment.Where(e => e != null).Sum(e => e.FlatMAtk);
    private float EquipMDefense() => equipment.Where(e => e != null).Sum(e => e.FlatMDef);
    private float EquipSpeedMul()
        => equipment.Where(e => e != null)
                    .Aggregate(1f, (acc, e) => acc * (1f + e.SpeedMul));
    private float EquipRangeMul()
        => equipment.Where(e => e != null)
                    .Aggregate(1f, (acc, e) => acc * (1f + e.RangeMul));
    private float EquipCooldownReduction()
        => equipment.Where(e => e != null)
                    .Aggregate(1f, (acc, e) => acc * (1f - e.CdReduction));
    private float EquipAttackRateMul()
        => equipment.Where(e => e != null)
                    .Aggregate(1f, (acc, e) => acc * (1f + e.AttackRateMul));
    
    public void SetLevel(int level)
    {
        this.level = level;
        OnLevelUp?.Invoke(this, level);
    }
    public void GiveExp(int addExp)
    {
        if (addExp <= 0 || baseData == null) return;

        int lastLevel = level;
        // Determine max level (fallback to 100 if your CharacterData doesn't expose one)
        int maxLevel = 100;
        // If your CharacterData has a max level field/property, prefer that:
        // maxLevel = Mathf.Max(1, baseData.maxLevel);

        accumulatedExp = Mathf.Clamp(accumulatedExp + addExp, int.MinValue, int.MaxValue);

        // Use long internally to avoid overflow for big grants
        long carry = addExp;

        // If already capped, ignore new EXP
        if (level >= maxLevel)
        {
            level = maxLevel;
            exp = 0;
            return;
        }

        // Add to current-progress EXP
        long current = exp + carry;

        // Consume EXP while enough to level up
        while (level < maxLevel)
        {
            int need = GetExpRequired(); // EXP needed from current level to next

            // Defensive: if your curve ever returns <= 0, break to avoid infinite loop
            if (need <= 0)
            {
                Debug.LogWarning($"GetExpRequired() returned {need} at level {level}. Clamping to 1.");
                need = 1;
            }

            if (current < need) break;

            current -= need;
            level++;
        }

        // If we hit cap, discard leftover and zero exp
        if (level >= maxLevel)
        {
            level = maxLevel;
            exp = 0;
            return;
        }

        // Otherwise keep leftover toward the next level (fits your segmented curve)
        exp = (int)Mathf.Clamp(current, 0, int.MaxValue);

        if (level != lastLevel)
        {
            OnLevelUp?.Invoke(this, level);
        }
        OnExpChange?.Invoke(this, exp);
    }

    public int GetExpRequired()
    {
        return CharacterData.GetRequiredExp(level);
    }
}

[Serializable]
public class EquipmentInstance
{
    public string equipId;
    public int level;
    public float FlatHP;
    public float FlatPAtk;
    public float FlatMAtk;
    public float FlatPDef;
    public float FlatMDef;
    public float SpeedMul;
    public float RangeMul;
    public float CdReduction;
    public float AttackRateMul; // e.g., 0.10 = +10%
}
