using System;
using UnityEngine;

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
    Dying
}

public class CharacterEntity : BaseEntity
{
    private Animator anim;

    public CharacterData characterData;
    public Team team;

    [Header("Movement Settings")]
    private Rigidbody2D rb;
    [SerializeField] private CharacterState state;
    public Orientation orientation;

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldownTimer = 0;
    [SerializeField] private CharacterEntity attackTarget = null;
    public bool debug = false;
    public bool targettingTower = false;
    public Tower targettedTower = null;
    public GameObject hitEffect;
    public bool towerInRange = false;

    [Header("Health")]
    public float currentHP;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Start()
    {
        AutoFlip();
        InitializeEntity();
    }

    private void Update()
    {
        BehaviorUpdate();
        AutoAttack();
        AnimationUpdate();
    }

    private void FixedUpdate()
    {
        if (!rb) return;
        if (state == CharacterState.Advancing)
        {
            rb.linearVelocityX = Time.fixedDeltaTime * characterData.moveSpeed * ((team == Team.Friendly) ? 1 : -1);
        } else
        {
            rb.linearVelocityX = 0;
        }
    }

    private void InitializeEntity()
    {
        MaxHealth = characterData.hp;
        ResetHealth();

        attackCooldownTimer = 0.0f;
    }

    void SwitchState(CharacterState state)
    {
        this.state = state;
    }

    void AnimationUpdate()
    {
        anim.SetBool("Running", state == CharacterState.Advancing);
    }

    void BehaviorUpdate()
    {
        var container = CharacterContainer.Instance;
        if (container == null)
        {
            SwitchState(CharacterState.None);
            attackTarget = null;
            return;
        }

        var enemy = container.GetFrontMost(team == Team.Friendly ? Team.Hostile : Team.Friendly); // Get Hostile if its friendly, vice versa
        if (enemy == null)
        {
            // Target Enemy Tower Instead
            if (!targettingTower || !targettedTower)
            {
                targettingTower = true;
                targettedTower = team == Team.Friendly ? MapController.Instance.spawnedEnemyBase : MapController.Instance.spawnedPlayerBase;
            }

            if (targettedTower != null)
            {
                var distToTower = Mathf.Abs(targettedTower.GetXPosition() - transform.position.x);
                if (distToTower <= characterData.range)
                {
                    towerInRange = true;
                    SwitchState(CharacterState.Attacking);
                    return;
                }
            }
            if (attackCooldownTimer <= 0)
            {
                SwitchState(CharacterState.Advancing);
            }
            else
            {
                SwitchState(CharacterState.None);
            }
                attackTarget = null;
            return;
        }

        targettingTower = false;
        targettedTower = null;
        towerInRange = false;
        var distance = Mathf.Abs(transform.position.x - enemy.transform.position.x);

        if (distance <= characterData.range)
        {
            SwitchState(CharacterState.Attacking);
            attackTarget = enemy;
        }
        else
        {
            SwitchState(CharacterState.Advancing);
        }
    }

    public void SetTeam(Team t)
    {
        this.team = t;
    }

    private void AutoFlip()
    {
        orientation = team == Team.Friendly ? Orientation.Right : Orientation.Left;
        transform.localRotation = Quaternion.Euler(new Vector3(transform.localRotation.x, team == Team.Friendly ? 0 : 180, transform.localRotation.z));
    }

    private void AutoAttack()
    {
        attackCooldownTimer -= Time.deltaTime;

        if (state == CharacterState.Attacking)
        {
            if (attackCooldownTimer <= 0)
            {
                anim.SetTrigger("Attack");
                attackCooldownTimer = characterData.AttackCooldown();
            }
        }
    }

    public void AttackTarget()
    {
        if (attackTarget == null)
        {
            if (targettingTower && targettedTower != null && towerInRange)
            {
                targettedTower.Damage(characterData.damage);
                Instantiate(hitEffect, (Vector2)targettedTower.transform.position + RandomOffset(), Quaternion.identity);
            }
        } else
        {
            attackTarget.Damage(characterData.damage);
            Instantiate(hitEffect, (Vector2)attackTarget.transform.position + RandomOffset(), Quaternion.identity);
        }
    }

    public Vector2 RandomOffset()
    {
        return new Vector2(UnityEngine.Random.Range(-0.1f, 0.1f), UnityEngine.Random.Range(-0.1f, 0.1f));
    }
}
