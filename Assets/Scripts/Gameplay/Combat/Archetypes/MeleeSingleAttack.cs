using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack/Melee Single")]
public class MeleeSingleAttack : AttackBehavior
{

    public override void Execute(CharacterEntity attacker)
    {
        var attackTarget = attacker.GetAttackTarget();
        if (attackTarget == null)
            return;

        attackTarget.Damage(
            attacker.characterInstance.GetStat(CharacterStatType.PAtk)
        );

        if (attacker.hitEffect != null)
        {
            Instantiate(
                attacker.hitEffect,
                attackTarget.transform.position + attackTarget.MidBodyOffset(),
                Quaternion.identity
            );
        }
    }
}
