using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] protected Health health;
    [SerializeField] protected KnockbackReceiver knockbackReceiver;
    [SerializeField] protected HitFeedback hitFeedback;

    public bool IsDead => health != null && health.IsDead;

    private void Awake()
    {
        health = GetComponent<Health>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        hitFeedback = GetComponent<HitFeedback>();
    }

    private void Start()
    {
        IgnorePlayerCollision();
    }

    private void IgnorePlayerCollision()
    {
        if (!TryGetComponent<Collider2D>(out var myCollider))
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            return;
        }

        Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>();

        foreach (var playerCol in playerColliders)
        {
            Physics2D.IgnoreCollision(myCollider, playerCol, true);
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
