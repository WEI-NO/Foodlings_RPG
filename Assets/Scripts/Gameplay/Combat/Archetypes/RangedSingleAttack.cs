using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack/Ranged Single")]
public class RangedSingleAttack : AttackBehavior
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;

    public override void Execute(CharacterEntity attacker)
    {
        if (attacker.attackTarget == null || projectilePrefab == null)
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            attacker.transform.position,
            Quaternion.identity
        );

        //Projectile proj = projectile.GetComponent<Projectile>();
        //proj.Init(
        //    attacker.attackTarget,
        //    attacker.characterInstance.GetStat(CharacterStatType.PAtk),
        //    projectileSpeed,
        //    attacker.team
        //);
    }
}