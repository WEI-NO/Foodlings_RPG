using UnityEngine;

public abstract class AttackBehavior : ScriptableObject
{
    /// <summary>
    /// Called by CharacterEntity via animation event.
    /// </summary>
    public abstract void Execute(CharacterEntity attacker);

    protected void HitEffect(CharacterEntity attacker, BaseEntity target)
    {
        if (attacker.hitEffect != null)
        {
            Instantiate(
                attacker.hitEffect,
                target.transform.position + new Vector3(0, target.height / 2.0f, 0),
                Quaternion.identity
            );
        }
    }

    public virtual bool CanAttack(CharacterEntity attacker)
    {
        return true;
    }

    protected BaseEntity FindTarget(CharacterEntity attacker)
    {
        if (attacker == null) return null;
        return attacker.targettingTower ? attacker.targettedTower : attacker.attackTarget;
    }
}