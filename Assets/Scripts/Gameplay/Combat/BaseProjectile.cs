using Unity.VisualScripting;
using UnityEngine;

public abstract class BaseProjectile : MonoBehaviour
{
    // -----------------------
    // Visual
    // -----------------------
    [Header("Visual")]
    public Sprite projectileSprite;
    protected SpriteRenderer spriteRenderer;
    public bool animationForDestroy = false;

    protected void OrientToDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    // -----------------------
    // Combat Data
    // -----------------------
    protected float damage;
    protected float speed;
    protected Team team;
    protected BaseEntity target;
    protected CharacterEntity owner;
    public float impactRadius = 1.0f;
    public LayerMask impactMask;

    public bool AOE;
    public int MaxPenetrationCount = 1;
    protected int PenetratedCount = 0;

    protected bool initialized = false;

    // -----------------------
    // Unity Lifecycle
    // -----------------------
    protected virtual void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && projectileSprite != null)
        {
            spriteRenderer.sprite = projectileSprite;
        }

        if (animationForDestroy)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.Update(0f); // Forces first animation frame immediately
            }
        }

    }

    protected virtual void Update()
    {
        if (!initialized)
            return;

        //if (target == null)
        //{
        //    DestroyProjectile();
        //    return;
        //}

        Move();
    }

    // -----------------------
    // Initialization
    // -----------------------
    public virtual void Init(
        CharacterEntity owner,
        BaseEntity target,
        float damage,
        float speed,
        Team team)
    {
        this.owner = owner;
        this.target = target;
        this.damage = damage;
        this.speed = speed;
        this.team = team;
        PenetratedCount = 0;

        initialized = true;
    }

    // -----------------------
    // Movement
    // -----------------------
    protected abstract void Move();

    // -----------------------
    // Impact / Damage
    // -----------------------
    protected virtual void OnImpact(BaseEntity entity)
    {
        if (entity != null && CanHit(entity))
        {
            entity.Damage(damage);
        }

        Destroy(gameObject);
    }

    protected void OnImpact_Internal()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            impactRadius,
            impactMask
        );

        if (AOE)
        {
            // -----------------------
            // AREA DAMAGE
            // -----------------------
            foreach (var hit in hits)
            {
                BaseEntity entity = hit.GetComponentInParent<BaseEntity>();
                if (entity == null) continue;
                if (!CanHit(entity)) continue;

                entity.Damage(damage);

                // Spawn hit effect per enemy hit
                if (owner != null && owner.hitEffect != null)
                {
                    Instantiate(
                        owner.hitEffect,
                        entity.transform.position + entity.MidBodyOffset(),
                        Quaternion.identity
                    );
                }
            }
        }
        else
        {
            // -----------------------
            // SINGLE TARGET (CLOSEST TO TOWER BY X)
            // -----------------------
            BaseEntity bestTarget = null;
            float bestValue = (team == Team.Friendly) ? float.MaxValue : float.MinValue;

            foreach (var hit in hits)
            {
                BaseEntity entity = hit.GetComponentInParent<BaseEntity>();
                if (entity == null) continue;
                if (!CanHit(entity)) continue;

                float enemyX = entity.transform.position.x;

                if (team == Team.Friendly)
                {
                    // Friendly moves right, so choose the smallest X (closest to friendly tower)
                    if (enemyX < bestValue)
                    {
                        bestValue = enemyX;
                        bestTarget = entity;
                    }
                }
                else
                {
                    // Hostile moves left, so choose the largest X
                    if (enemyX > bestValue)
                    {
                        bestValue = enemyX;
                        bestTarget = entity;
                    }
                }
            }

            if (bestTarget != null)
            {
                bestTarget.Damage(damage);

                // Spawn hit effect only on the enemy actually hit
                if (owner != null && owner.hitEffect != null)
                {
                    Instantiate(
                        owner.hitEffect,
                        bestTarget.transform.position + bestTarget.MidBodyOffset(),
                        Quaternion.identity
                    );
                }

                RegisterPenetrationHit();
            }
        }

        DestroyProjectile();
    }


    protected virtual bool CanHit(BaseEntity entity)
    {
        if (entity is CharacterEntity character)
            return character.team != team;

        return true;
    }

    protected virtual bool ValidProjectile()
    {
        if (PenetratedCount >= MaxPenetrationCount)
        {
            return false;
        }

        return true;
    }

    protected void RegisterPenetrationHit()
    {
        PenetratedCount++;
    }

    // -----------------------
    // Destroy
    // -----------------------
    protected virtual void DestroyProjectile()
    {
        if (animationForDestroy)
        {
            var anim = GetComponent<Animator>();
            if (anim)
            {
                anim.SetTrigger("Destroy");
                return;
            }
        }

        DestroyProjectile_Internal();
    }

    public void DestroyProjectile_Internal()
    {
        Destroy(gameObject);
    }
}
