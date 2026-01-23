using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack/Melee Area")]
public class MeleeAreaAttack : AttackBehavior
{
    public float radius = 1.5f;
    public Vector3 offset = new();
    public LayerMask targetMask;

    public override void Execute(CharacterEntity attacker)
    {
        var offset_team = ConvertOffset(attacker, offset);
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attacker.transform.position + attacker.MidBodyOffset() + offset_team,
            radius,
            targetMask
        );

        int hitCount = 0;
        foreach (var hit in hits)
        {
            var entity = hit.GetComponentInParent<CharacterEntity>();
            if (entity == null || entity.team == attacker.team)
                continue;

            HitEffect(attacker, entity);

            entity.Damage(
                attacker.characterInstance.GetStat(CharacterStatType.MAtk)
            );

            hitCount++;
        }

        if (attacker.targettingTower)
        {
            HitEffect(attacker, attacker.targettedTower);
            attacker.targettedTower.Damage(attacker.characterInstance.GetStat(CharacterStatType.PAtk));
        }

    }
}
