using UnityEngine;


[CreateAssetMenu(menuName = "Combat/Attack/Ranged Area")]
public class RangedAreaAttack : AttackBehavior
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 6f;
    public float explosionRadius = 2f;
    public LayerMask targetMask;

    public override void Execute(CharacterEntity attacker)
    {
        if (attacker.attackTarget == null || projectilePrefab == null)
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            attacker.transform.position,
            Quaternion.identity
        );

        //AreaProjectile proj = projectile.GetComponent<AreaProjectile>();
        //proj.Init(
        //    attacker.attackTarget.Position,
        //    attacker.characterInstance.GetStat(CharacterStatType.PAtk),
        //    explosionRadius,
        //    projectileSpeed,
        //    attacker.team,
        //    targetMask
        //);
    }
}
