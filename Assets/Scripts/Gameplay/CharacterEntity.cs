using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public enum Team
{
    Friendly,
    Hostile
}

public enum Orientation
{
    Left,
    Right
}

public enum CharacterState
{
    None,
    Advancing,
    Attacking,
    Knocked,
    Dying
}

public class CharacterEntity : BaseEntity
{
    // -----------------------
    // Components & Identity
    // -----------------------
    private Animator anim;
    private Rigidbody2D rb;

    public CharacterInstance characterInstance;
    public Team team;
    public Orientation orientation;

    [Header("State")]
    [SerializeField] public CharacterState state;
    public bool debug = false;

    // -----------------------
    // Combat
    // -----------------------
    [Header("Combat Settings")]
    public AttackBehavior attackBehavior;
    [SerializeField] private float attackCooldownTimer = 0;
    public CharacterEntity attackTarget = null;
    public bool targettingTower = false;
    public Tower targettedTower = null;
    public GameObject hitEffect;
    public bool towerInRange = false;

    public float effectYOffset = 0.2f;

    // -----------------------
    // Health
    // -----------------------
    [Header("Health")]
    public float currentHP;
    public int knockbackIndex = 0;

    // -----------------------
    // Knockback
    // -----------------------
    [Header("Knockback Settings")]
    private float knockbackDistance = 1.5f;   // how far the first bounce goes
    private float knockbackHeight = 0.35f;    // how high the first bounce goes

    public float knockbackDuration = 1.2f;
    private bool isKnockback = false;
    private Coroutine knockbackRoutine;

    public bool inAttackAnimation = false;



    // =======================
    // Unity Lifecycle
    // =======================
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        knockbackIndex = 0;
    }

    private void Start()
    {
        AutoFlip();
        InitializeEntity();
    }

    private void Update()
    {
        inAttackAnimation = IsAttackAnimationPlaying();

        if (isKnockback)
        {
            HandleKnockbackUpdate();
        }
        else
        {
            HandleNormalUpdate();
        }
    }

    private void FixedUpdate()
    {
        if (!rb) return;

        if (isKnockback)
        {
            rb.linearVelocityX = 0f;
            return;
        }

        if (IsAttackAnimationPlaying() && state == CharacterState.Attacking)
        {
            rb.linearVelocityX = 0f;
            return;
        }

        HandleMovement();
    }

    // =======================
    // Update Helpers
    // =======================
    private void HandleKnockbackUpdate()
    {
        SwitchState(CharacterState.Knocked);
        AnimationUpdate();
    }

    private void HandleNormalUpdate()
    {
        BehaviorUpdate();
        AutoAttack();
        AnimationUpdate();
    }

    private void HandleMovement()
    {
        if (state == CharacterState.Advancing)
        {
            rb.linearVelocityX =
                Time.fixedDeltaTime *
                characterInstance.GetStat(CharacterStatType.Spe) *
                GetForwardDirection();
        }
        else
        {
            rb.linearVelocityX = 0;
        }
    }

    // =======================
    // Damage / Health
    // =======================
    protected override void OnDamage_Internal(float damage)
    {
        if (characterInstance.baseData.healthThreshold.Length <= knockbackIndex)
            return;

        float threshold = MaxHealth * characterInstance.baseData.healthThreshold[knockbackIndex];
        if (CurrentHealth <= threshold)
        {
            StartKnockback(knockbackDuration);
            knockbackIndex++;
        }
    }

    private void InitializeEntity()
    {
        MaxHealth = characterInstance.GetStat(CharacterStatType.HP);
        ResetHealth();

        attackBehavior = characterInstance.baseData.behavior;
        attackCooldownTimer = 0.0f;
    }

    // =======================
    // State & Animation
    // =======================
    private void SwitchState(CharacterState newState)
    {
        state = newState;
    }

    private void AnimationUpdate()
    {
        bool inAttack = IsAttackAnimationPlaying();

        // If we're attacking, force Running false so it doesn't blend into run/idle
        if (inAttack)
        {
            anim.SetBool("Running", false);
            return;
        }

        anim.SetBool("Running", state == CharacterState.Advancing);
    }


    // =======================
    // Behavior / AI
    // =======================
    private void BehaviorUpdate()
    {
        // If we're in the attack animation, don't change AI state
        if (IsAttackAnimationPlaying() && state == CharacterState.Attacking)
        {
            // Waiting for attack
            return;
        }

        var container = CharacterContainer.Instance;
        if (container == null)
        {
            SwitchState(CharacterState.None);
            attackTarget = null;
            return;
        }

        if (state == CharacterState.Knocked)
        {
            rb.linearVelocityX = 0;
            return;
        }

        var enemy = container.GetFrontMost(OpposingTeam());

        if (enemy == null)
        {
            HandleNoEnemy();
        }
        else
        {
            HandleEnemyPresent(enemy);
        }
    }

    private void HandleNoEnemy()
    {
        // Target Enemy Tower Instead
        if (!targettingTower || !targettedTower)
        {
            targettingTower = true;
            targettedTower = team == Team.Friendly
                ? MapController.Instance.spawnedEnemyBase
                : MapController.Instance.spawnedPlayerBase;
        }

        if (targettedTower != null)
        {
            var distToTower = Mathf.Abs(targettedTower.GetXPosition() - transform.position.x);
            if (distToTower <= characterInstance.GetStat(CharacterStatType.AtkRng))
            {
                towerInRange = true;
                SwitchState(CharacterState.Attacking);
                return;
            }
        }

        AdvancingStateCheck();
        attackTarget = null;
    }

    private void HandleEnemyPresent(CharacterEntity enemy)
    {
        targettingTower = false;
        towerInRange = false;

        var enemyDist = Mathf.Abs(transform.position.x - enemy.transform.position.x);

        Tower myTower = (team == Team.Friendly)
            ? MapController.Instance.spawnedEnemyBase
            : MapController.Instance.spawnedPlayerBase;


        if (IsEnemyBehindTower(enemy, myTower))
        {
            targettedTower = myTower;
            attackTarget = null;

            float distToTower = Mathf.Abs(myTower.GetXPosition() - transform.position.x);

            if (distToTower <= characterInstance.GetStat(CharacterStatType.AtkRng))
            {
                SwitchState(CharacterState.Attacking);
                towerInRange = true;
                targettingTower = true;
            }
            else
            {
                AdvancingStateCheck();
            }

            return; // stop processing enemy
        }

        // ============================
        // Normal enemy targeting logic
        // ============================
        targettingTower = false;
        float attackRange = characterInstance.GetStat(CharacterStatType.AtkRng);

        if (enemyDist <= attackRange)
        {
            SwitchState(CharacterState.Attacking);
            attackTarget = enemy;
        }
        else
        {
            AdvancingStateCheck();
        }
    }

    private void AdvancingStateCheck()
    {
        if (inAttackAnimation)
        {
            SwitchState(CharacterState.None);
            return;
        }

        if (characterInstance.baseData.onlyMoveWhenAttackReady)
        {
            if (attackCooldownTimer <= 0)
            {
                SwitchState(CharacterState.Advancing);
                return;
            }
        }
        else
        {
            SwitchState(CharacterState.Advancing);
            return;
        }

        SwitchState(CharacterState.None);
    }

    private bool IsAttackAnimationPlaying()
    {
        if (anim == null) return false;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag("attack");   // uses the Tag we set in the Animator
    }

    // =======================
    // Identity / Setup
    // =======================
    public void SetTeam(Team t) => team = t;

    public void SetCharacterInstance(CharacterInstance instance) =>
        characterInstance = instance;

    private void AutoFlip()
    {
        orientation = team == Team.Friendly ? Orientation.Right : Orientation.Left;

        transform.localRotation = Quaternion.Euler(
            new Vector3(
                transform.localRotation.x,
                team == Team.Friendly ? 0 : 180,
                transform.localRotation.z));
    }

    // =======================
    // Combat
    // =======================
    private void AutoAttack()
    {
        attackCooldownTimer -= Time.deltaTime;

        if (state == CharacterState.Attacking && attackCooldownTimer <= 0)
        {
            anim.SetTrigger("Attack");
            attackCooldownTimer = 1 / characterInstance.GetStat(CharacterStatType.AtkSpe);
            // Random Offset
            attackCooldownTimer += UnityEngine.Random.Range(0f, 0.1f);
        }
    }

    /// <summary>
    /// Called by animation event on attack frame
    /// </summary>
    public void OnAttackFrame()
    {
        if (attackBehavior == null)
            return;

        // Knocked
        if (!ValidTarget())
        {
            attackTarget = CharacterContainer.Instance.GetFrontMost(OpposingTeam());
            if (attackTarget == null) return;
            var enemyDist = Mathf.Abs(transform.position.x - attackTarget.transform.position.x);
            if (enemyDist > characterInstance.GetStat(CharacterStatType.AtkRng))
            {
                attackTarget = null;
                return;
            }
        }

        if (!attackBehavior.CanAttack(this))
            return;

        attackBehavior.Execute(this);
    }

    //public void AttackTarget()
    //{
    //    if (attackTarget == null)
    //    {
    //        if (targettingTower && targettedTower != null && towerInRange)
    //        {
    //            targettedTower.Damage(characterInstance.GetStat(CharacterStatType.PAtk));
    //            Instantiate(hitEffect,
    //                (Vector2)targettedTower.transform.position + new Vector2(0, 1.0f) + RandomOffset(),
    //                Quaternion.identity);
    //        }
    //    }
    //    else
    //    {
    //        attackTarget.Damage(characterInstance.GetStat(CharacterStatType.PAtk));
    //        Instantiate(hitEffect,
    //            (Vector2)attackTarget.transform.position + new Vector2(0, effectYOffset) + RandomOffset(),
    //            Quaternion.identity);
    //    }
    //}

    public Vector2 RandomOffset()
    {
        return new Vector2(
            UnityEngine.Random.Range(-0.1f, 0.1f),
            UnityEngine.Random.Range(-0.1f, 0.1f));
    }

    // =======================
    // Knockback (Public API)
    // =======================
    public void StartKnockback(float duration)
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(Knockback(duration));
    }

    // =======================
    // Knockback (Coroutine)
    // =======================
    private IEnumerator Knockback(float duration)
    {
        isKnockback = true;
        anim.SetBool("Knocked", true);
        SwitchState(CharacterState.Knocked);

        float dir = GetKnockbackDirection();

        float elapsed = 0f;
        float startSpeed = knockbackDistance / Mathf.Max(0.01f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease-out curve (fast → slow)
            float speed = Mathf.Lerp(startSpeed, 0f, t);

            transform.position += new Vector3(dir * speed * Time.deltaTime, 0f, 0f);
            yield return null;
        }

        isKnockback = false;
        anim.SetBool("Knocked", false);
        SwitchState(CharacterState.None);
    }


    private IEnumerator KnockbackBounce(float dir, float duration, float distance, float height)
    {
        float t = 0f;

        Vector3 start = transform.position;
        float x0 = start.x;
        float y0 = start.y;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);

            // Horizontal: smooth out
            float x = x0 + dir * Mathf.Lerp(0f, distance, a);

            // Vertical: smooth arc (0 at start/end, peak in middle)
            // Sin(pi*a) gives 0 -> 1 -> 0 smoothly
            float y = y0 + Mathf.Sin(Mathf.PI * a) * height;

            transform.position = new Vector3(x, y, start.z);
            yield return null;
        }

        // Ensure exact landing
        transform.position = new Vector3(x0 + dir * distance, y0, start.z);
    }

    // =======================
    // Direction Helpers
    // =======================
    private float GetForwardDirection()
    {
        // Matches your previous logic:
        // Friendly moves +X, Hostile moves -X
        return (team == Team.Friendly) ? 1f : -1f;
    }

    private float GetKnockbackDirection()
    {
        // Knockback is opposite of forward
        return -GetForwardDirection();
    }

    private bool IsEnemyBehindTower(CharacterEntity enemy, Tower tower)
    {
        if (enemy == null || tower == null)
            return false;

        float enemyX = enemy.transform.position.x;
        float towerX = tower.GetXPosition();

        // Friendly moves +X → tower is on right
        // Hostile  moves -X → tower is on left
        bool behind = (team == Team.Friendly)
            ? enemyX > towerX        // enemy is to the right → behind tower
            : enemyX < towerX;       // enemy is to the left → behind tower

        return behind;
    }

    protected override void OnDeath()
    {
        if (knockbackRoutine != null)
        {
            StopCoroutine(knockbackRoutine);
        }
        knockbackRoutine = StartCoroutine(OnDeathKnock());
    }

    private IEnumerator OnDeathKnock()
    {
        anim.SetBool("Dead", true);
        yield return Knockback(knockbackDuration);

        if (CharacterContainer.Instance != null && CharacterContainer.Instance.deathObject != null)
        {
            var ghost = Instantiate(CharacterContainer.Instance.deathObject);
            ghost.transform.position = transform.position;
        }

        base.OnDeath();
    }

    public BaseEntity GetAttackTarget()
    {
        if (targettingTower && towerInRange)
        {
            return targettedTower;
        }
        return attackTarget;
    }

    private bool ValidTarget()
    {
        var target = GetAttackTarget();
        if (target is CharacterEntity)
        {
            var ce = target as CharacterEntity;
            if (ce.state == CharacterState.Knocked)
            {
                return false;
            }
        }
        return true;
    }

    private Team OpposingTeam()
    {
        return team == Team.Friendly ? Team.Hostile : Team.Friendly;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (attackBehavior == null)
            return;

        // Only draw for MeleeAreaAttack
        if (attackBehavior is MeleeAreaAttack areaAttack)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.35f); // semi-transparent red
            var offset_team = AttackBehavior.ConvertOffset(this, areaAttack.offset);

            Gizmos.DrawWireSphere(transform.position + MidBodyOffset() + offset_team, areaAttack.radius);
        }
    }
#endif

}
