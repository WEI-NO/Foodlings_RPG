using System;
using UnityEngine;

public class BaseEntity : MonoBehaviour
{
    [Header("Health Properties")]
    [SerializeField] protected float MaxHealth;
    public float CurrentHealth;

    [Header("Identity Properties")]
    public Action<BaseEntity> OnEntityDead;

    [Header("Combat Properties")]
    public GameObject attackProjectile;
    protected bool isDead = false;

    public void Damage(float damage)
    {
        CurrentHealth -= damage;

        if (CurrentHealth  <= 0)
        {
            OnDeath_Internal();
            CurrentHealth = 0;
        }
    }

    protected void OnDeath_Internal()
    {
        if (isDead) return;
        OnEntityDead?.Invoke(this);
        OnDeath();
        isDead = true;
    }
        
    protected virtual void OnDeath() 
    {
        Destroy(gameObject);
    }

    protected void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
}
