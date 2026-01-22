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
        if ((attacker.attackTarget == null && !attacker.targettingTower) || projectilePrefab == null)
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            attacker.transform.position,
            Quaternion.identity
        );

        BaseProjectile proj = projectile.GetComponent<BaseProjectile>();
        proj.Init(
            attacker,
            FindTarget(attacker),
            attacker.characterInstance.GetStat(CharacterStatType.MAtk),
            projectileSpeed,
            attacker.team
        );
    }
}
