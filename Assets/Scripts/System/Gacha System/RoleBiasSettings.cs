using UnityEngine;

[CreateAssetMenu(menuName = "Gacha/Role Bias Settings")]
public class RoleBiasSettings : ScriptableObject
{
    [Range(0f, 0.5f)]
    public float supportChance = 0.05f;

    [Min(0f)]
    public float baseWeight = 1f;

    public Role Calculate(GachaRequest request)
    {
        float fighterW = baseWeight + request.GetStage(Role.Fighter);
        float tankW = baseWeight + request.GetStage(Role.Tank);
        float magicW = baseWeight + request.GetStage(Role.Magic);

        float sum = fighterW + tankW + magicW;
        float remain = 1f - supportChance;

        float fighterChance;
        float tankChance;
        float magicChance;

        if (sum <= 0.0001f)
        {
            fighterChance = remain / 3f;
            tankChance = remain / 3f;
            magicChance = remain / 3f;
        }
        else
        {
            fighterChance = remain * (fighterW / sum);
            tankChance = remain * (tankW / sum);
            magicChance = remain * (magicW / sum);
        }

        float roll = Random.value;
        float cumulative = 0f;

        cumulative += fighterChance;
        if (roll < cumulative) return Role.Fighter;

        cumulative += tankChance;
        if (roll < cumulative) return Role.Tank;

        cumulative += magicChance;
        if (roll < cumulative) return Role.Magic;

        return Role.Support;
    }

}

[System.Serializable]
public struct RoleChances
{
    [Range(0f, 1f)] public float fighter;
    [Range(0f, 1f)] public float tank;
    [Range(0f, 1f)] public float magic;
    [Range(0f, 1f)] public float support;
}
