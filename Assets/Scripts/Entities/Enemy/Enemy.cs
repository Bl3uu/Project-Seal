using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Data")]
    [SerializeField] protected EnemyData enemyData;

    [Header("References")]
    [SerializeField] protected Health health;
    [SerializeField] protected Poise poise;
    [SerializeField] protected KnockbackReceiver knockbackReceiver;
    [SerializeField] protected HitFeedback hitFeedback;

    public bool IsDead => health != null && health.IsDead;
    public EnemyData Data => enemyData;

    private void Awake()
    {
        health = GetComponent<Health>();
        poise = GetComponent<Poise>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        hitFeedback = GetComponent<HitFeedback>();

        if (enemyData != null)
        {
            ApplyEnemyData();
        }
    }

    protected virtual void ApplyEnemyData()
    {
        if (health != null)
        {
            health.SetMaxHealth(enemyData.maxHealth);
        }

        if (poise != null)
        {
            poise.Initialize(enemyData.maxPoise);
        }
    }

    public virtual void TakeDamage(DamageData payload)
    {
        if (IsDead)
        {
            return;
        }

        if (health != null)
        {
            health.ApplyDamage(payload.Amount);
        }

        if (poise != null)
        {
            poise.ApplyPoiseDamage(payload.PoiseDamage);
        }

        if (knockbackReceiver != null)
        {
            knockbackReceiver.ApplyKnockback(payload.HitDirection, payload.KnockbackForce, payload.KnockbackDuration);
        }

        if (hitFeedback != null)
        {
            hitFeedback.PlayHitEffects();
        }
    }
    
    private void HandleDeath()
    {
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDeath.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDeath.RemoveListener(HandleDeath);
        }
    }
}
