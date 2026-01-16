using System;
using System.Collections;
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

    [Header("Effect Setting")]
    public float height = 1.0f;
    [Header("Hurt Effect")]
    public SpriteRenderer spriteRenderer;
    private Coroutine hitFlashRoutine;

    public Vector3 MidBodyOffset()
    {
        return new Vector3(0, height / 2.0f, 0);
    }

    public void Damage(float damage)
    {
        CurrentHealth -= damage;
        OnDamage_Internal(damage);
        PlayHitFlash();

        if (CurrentHealth  <= 0)
        {
            OnDeath_Internal();
            CurrentHealth = 0;
        }
    }

    protected virtual void OnDamage_Internal(float damage)
    {

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

    protected void PlayHitFlash()
    {
        if (spriteRenderer == null)
            return;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);

        hitFlashRoutine = StartCoroutine(HitFlashCoroutine());
    }

    private IEnumerator HitFlashCoroutine()
    {
        Color hitColor = new Color(150 / 255f, 50 / 255f, 40 / 255f, 1.0f);
        Color originalColor = Color.white;

        // Instantly go to hit color
        spriteRenderer.color = hitColor;

        float duration = 0.1f;
        float elapsed = 0f;

        // Gradually fade back to white
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            spriteRenderer.color = Color.Lerp(hitColor, originalColor, t);
            yield return null;
        }

        // Ensure final color is restored
        spriteRenderer.color = originalColor;
        hitFlashRoutine = null;
    }

}
