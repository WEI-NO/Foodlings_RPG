using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack/Ranged Single")]
public class RangedSingleAttack : AttackBehavior
{
    public GameObject projectilePrefab;
    public float spawnYOffset = 0.0f;
    public float projectileSpeed = 2f;

    public override void Execute(CharacterEntity attacker)
    {
        if ((attacker.attackTarget == null && !attacker.targettingTower) || projectilePrefab == null)
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            attacker.transform.position + new Vector3(0, spawnYOffset, 0),
            Quaternion.identity
        );

        BaseProjectile proj = projectile.GetComponent<BaseProjectile>();
        proj.Init(
            attacker,
            attacker.targettingTower ? attacker.targettedTower : attacker.attackTarget,
            attacker.characterInstance.GetStat(CharacterStatType.PAtk),
            projectileSpeed,
            attacker.team
        );
    }
}