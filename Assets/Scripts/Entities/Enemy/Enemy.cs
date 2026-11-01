using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable, IKnockbackable
{
    [Header("Data")]
    [SerializeField] protected EnemyData enemyData;

    [Header("References")]
    [SerializeField] protected Health health;
    [SerializeField] protected Poise poise;
    [SerializeField] protected KnockbackReceiver knockbackReceiver;
    [SerializeField] protected HitFeedback hitFeedback;
    [SerializeField] protected EntityMotor motor;
    [SerializeField] protected Rigidbody2D rb;
    private Collider2D col;

    public IEnemyState CurrentState { get; protected set; }
    public EnemyStaggeredState StaggeredState { get; private set; }
    public EnemyKnockbackState KnockbackState { get; private set; }

    public float healthPercentage;
    public bool IsDead => health != null && health.IsDead;
    public EnemyData Data => enemyData;
    public Health Health => health;
    public float HealthPercentage => healthPercentage;

    private void Awake()
    {
        health = GetComponent<Health>();
        poise = GetComponent<Poise>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        hitFeedback = GetComponent<HitFeedback>();
        motor = GetComponent<EntityMotor>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        StaggeredState = new EnemyStaggeredState(this, motor, 1.5f);
        KnockbackState = new EnemyKnockbackState(this, motor, rb);

        if (enemyData != null)
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

        if (health != null && health.MaxHealth > 0)
        {
            healthPercentage = health.CurrentHealth / health.MaxHealth;
        }
        else
        {
            healthPercentage = 1f;
        }
    }

    private void Update()
    {
        if (IsDead)
        {
            return;
        }

        if (CurrentState != null)
        {
            CurrentState.UpdateState();
        }
    }

    private void FixedUpdate()
    {
        if (IsDead)
        {
            return;
        }

        if (CurrentState != null)
        {
            CurrentState.FixedUpdateState();
        }
    }

    public virtual void ChangeState(IEnemyState newState)
    {
        if (newState == CurrentState)
        {
            return;
        }

        string oldStateName = CurrentState != null ? CurrentState.GetType().Name : "NULL";
        string newStateName = newState != null ? newState.GetType().Name : "NULL";

        Debug.Log($"[Enemy State] {gameObject.name}: Changing state from **{oldStateName}** to **{newStateName}**", this);

        if (CurrentState != null)
        {
            CurrentState.Exit();
        }
        CurrentState = newState;

        if (CurrentState != null)
        {
            CurrentState.Enter();
        }
    }

    public virtual void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        KnockbackState.SetupKnockback(direction, force, duration);
        ChangeState(KnockbackState);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CurrentState is EnemyKnockbackState knockbackState)
        {
            knockbackState.OnImpactCollision(collision);
        }
    }

    private void OnEnable()
    {
        if (poise != null)
        {
            poise.OnGuardBroken += HandleGuardBroken;
            poise.OnGuardRecovered += HandleGuardRecovered;
        }

        if (health != null)
        {
            health.OnDeath.AddListener(HandleDeath);
        }
    }

    private void OnDisable()
    {
        if (poise != null)
        {
            poise.OnGuardBroken -= HandleGuardBroken;
            poise.OnGuardRecovered -= HandleGuardRecovered;
        }
        if (health != null)
        {
            health.OnDeath.RemoveListener(HandleDeath);
        }
    }

    protected virtual void HandleGuardBroken()
    {
        ChangeState(StaggeredState);
    }

    protected virtual void HandleGuardRecovered() { }

    protected virtual void HandleDeath()
    {
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
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
}
