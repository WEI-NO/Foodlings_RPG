using JetBrains.Annotations;
using UnityEngine;

public class CornProjectile : BaseProjectile
{
    [Header("Arc")]
    public float arcHeight = 2f;
    public float minFlightTime = 0.25f;
    public float maxFlightTime = 1.0f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float time;
    private float duration;

    public override void Init(CharacterEntity owner, BaseEntity target, float damage, float speed, Team team)
    {
        base.Init(owner, target, damage, speed, team);
        OrientToDirection(Vector2.up);
        startPos = transform.position;

        // Initial locked target position (enemy head)
        Vector3 rawTargetPos =
            target.transform.position + target.MidBodyOffset();

        // Determine max allowed landing X based on owner's attack range
        float maxRange = owner.characterInstance.GetStat(CharacterStatType.AtkRng);
        float direction = owner.team == Team.Friendly ? 1f : -1f;

        float maxAllowedX = owner.transform.position.x + direction * maxRange;

        // Clamp target X so projectile never lands beyond max range
        float clampedX = rawTargetPos.x;

        if (Mathf.Abs(rawTargetPos.x - owner.transform.position.x) > maxRange)
        {
            clampedX = maxAllowedX;
        }

        targetPos = new Vector3(
            clampedX,
            rawTargetPos.y,
            rawTargetPos.z
        );

        float distance = Mathf.Abs(targetPos.x - startPos.x);

        // Convert distance to time and clamp for consistent feel
        duration = distance / speed;
        duration = Mathf.Clamp(duration, minFlightTime, maxFlightTime);

        time = 0f;
        //lastPos = transform.position;
    }

    protected override void Move()
    {
        if (!ValidProjectile())
        {
            return;
        }

        time += Time.deltaTime;
        float t = Mathf.Clamp01(time / duration);

        // -----------------------
        // Vertical
        // -----------------------
        float height = Mathf.Sin(Mathf.PI * t) * arcHeight;
        float baseY = Mathf.Lerp(startPos.y, targetPos.y, t);
        float y = baseY + height;

        // -----------------------
        // Horizontal
        // -----------------------
        float x = Mathf.Lerp(startPos.x, targetPos.x, t);

        // Apply position
        Vector3 pos = new Vector3(x, y, transform.position.z);
        transform.position = pos;

        // -----------------------
        // Rotation
        // -----------------------
        float maxTilt = 15f; 

        // Create a smooth 0 -> 1 -> 0 curve over the flight
        float tiltFactor = Mathf.Sin(Mathf.PI * t);

        // Apply tilt
        float angle = 90f + (tiltFactor * maxTilt);
        transform.rotation = Quaternion.Euler(0f, 0f, angle);


        // -----------------------
        // Impact
        // -----------------------
        if (t >= 1f)
        {
            OnImpact_Internal();
        }
    }



    private void HandleImpact()
    {
        //Collider2D[] hits = Physics2D.OverlapCircleAll(
        //    transform.position,
        //    impactRadius,
        //    targetMask
        //);

        //BaseEntity bestTarget = null;
        //float bestDistance = float.MaxValue;

        //float impactX = transform.position.x;

        //foreach (var hit in hits)
        //{
        //    BaseEntity entity = hit.GetComponentInParent<BaseEntity>();
        //    if (entity == null) continue;
        //    if (!CanHit(entity)) continue;

        //    float enemyX = entity.transform.position.x;

        //    if (team == Team.Friendly)
        //    {
        //        if (enemyX < bestDistance)
        //        {
        //            bestDistance = enemyX;
        //            bestTarget = entity;
        //        }
        //    } else
        //    {
        //        if (enemyX > bestDistance)
        //        {
        //            bestDistance = enemyX;
        //            bestTarget = entity;
        //        }
        //    }

        //}

        //if (bestTarget != null)
        //{
        //    bestTarget.Damage(damage);

        //    if (owner != null && owner.hitEffect != null)
        //    {
        //        Instantiate(
        //            owner.hitEffect,
        //            bestTarget.transform.position + bestTarget.MidBodyOffset(),
        //            Quaternion.identity
        //        );
        //    }
        //    RegisterPenetrationHit();
        //}


        //if (animationOnImpact)
        //{
        //    DestroyProjectile();
        //}
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
#endif
}
