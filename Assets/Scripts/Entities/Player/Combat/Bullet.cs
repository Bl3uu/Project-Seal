using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody2D rb;
    private float damage;
    private float knockbackForce;
    private float knockbackDuration;
    private GameObject ownerSource;
    private Vector2 moveDirection;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 direction, float damage, float knockbackForce, float knockbackDuration, GameObject source)
    {
        this.moveDirection = direction.normalized;
        this.damage = damage;
        this.knockbackForce = knockbackForce;
        this.knockbackDuration = knockbackDuration;
        this.ownerSource = source;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (rb != null)
        {
            rb.linearVelocity = moveDirection * speed;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit || collision.gameObject == ownerSource || collision.transform.root.gameObject == ownerSource)
        {
            return;
        }

        if (collision.TryGetComponent<IDamageable>(out var damageable))
        {
            hasHit = true;

            DamageData payload;

            payload.Amount = damage;
            payload.HitDirection = moveDirection;
            payload.KnockbackForce = knockbackForce;
            payload.KnockbackDuration = knockbackDuration;
            payload.Source = ownerSource;

            damageable.TakeDamage(payload);
            Destroy(gameObject);
            return;
        }

        if (!collision.isTrigger)
        {
            hasHit = true;
            Destroy(gameObject);
        }
    }
}
