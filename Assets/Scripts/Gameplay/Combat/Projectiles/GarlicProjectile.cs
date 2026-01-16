using UnityEngine;

public class GarlicProjectile : BaseProjectile
{
    public float dropHeight = 3f;
    public float smashSpeed = 10f;

    [Header("Timing")]
    public float startDelay = 0.25f;     // delay before falling
    public float impactAnimTime = 0.2f;  // time to show impact animation

    private Vector3 targetPos;
    private float timer = 0f;

    private enum State
    {
        Delay,
        Falling,
        Impact
    }

    private State state = State.Delay;

    public override void Init(CharacterEntity owner, BaseEntity target, float damage, float speed, Team team)
    {
        base.Init(owner, target, damage, speed, team);

        // Lock the target position at attack time
        targetPos = target.transform.position + target.MidBodyOffset();

        // Start above the enemy
        transform.position = targetPos + Vector3.up * dropHeight;

        timer = 0f;
        state = State.Delay;
    }

    protected override void Move()
    {
        switch (state)
        {
            case State.Delay:
                HandleDelay();
                break;

            case State.Falling:
                HandleFalling();
                break;

            case State.Impact:
                HandleImpactAnimation();
                break;
        }
    }

    // -----------------------
    // 1. Delay / Wind-up
    // -----------------------
    private void HandleDelay()
    {
        timer += Time.deltaTime;
        if (timer >= startDelay)
        {
            timer = 0f;
            state = State.Falling;
        }
    }

    // -----------------------
    // 2. Smash Down
    // -----------------------
    private void HandleFalling()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            smashSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) <= 0.01f)
        {
            // Apply damage immediately on impact
            OnImpact_Internal();

            timer = 0f;
            state = State.Impact;
        }
    }

    // -----------------------
    // 3. Impact Animation
    // -----------------------
    private void HandleImpactAnimation()
    {
        timer += Time.deltaTime;

        // Let animation / effect play before destroying
        if (timer >= impactAnimTime)
        {
            DestroyProjectile();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
#endif
}
