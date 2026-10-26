using UnityEngine;

public class InfantryAI : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private EnemyAim enemyAim;
    [SerializeField] private EntityMotor motor;
    [SerializeField] private Poise poise;

    // State Instances
    public IEnemyState CurrentState { get; private set;  }
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }
    public EnemyStaggeredState StaggeredState { get; private set; }

    // Attack Properties
    public AttackData SelectedAttack { get; private set;  }
    public EnemyData Data => data;
    public bool CanAttack => attackCooldownTimer <= 0f;

    private float attackCooldownTimer;
    private EnemyData data;

    private void Awake()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }
        if (targetDetector == null)
        {
            targetDetector = GetComponent<TargetDetector>();
        }
        if (enemyAim == null)
        {
            enemyAim = GetComponent<EnemyAim>();
        }
        if (poise == null)
        {
            poise = GetComponent<Poise>();
        }
        if (motor == null)
        {
            motor = GetComponent<EntityMotor>();
        }

        // Attack property handling
        if (enemy != null)
        {
            data = enemy.Data;
        }

        IdleState = new EnemyIdleState(this, targetDetector);
        ChaseState = new EnemyChaseState(this, targetDetector, enemyAim, motor, Data);
        AttackState = new EnemyAttackState(this, targetDetector, motor);
        StaggeredState = new EnemyStaggeredState(motor);
    }

    private void Start()
    {
        ChangeState(IdleState);
    }

    private void OnEnable()
    {
        if (poise != null)
        {
            poise.OnGuardBroken += HandleGuardBroken;
            poise.OnGuardRecovered += HandleGuardRecovered;
        }
    }

    private void OnDisable()
    {
        if (poise != null) 
        { 
            poise.OnGuardBroken -= HandleGuardBroken;
            poise.OnGuardRecovered -= HandleGuardRecovered;
        }
    }

    private void Update()
    {
        if (enemy != null && enemy.IsDead)
        {
            return;
        }

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
        }

        if (CurrentState != null)
        {
            CurrentState.UpdateState();
        }
    }

    public void ChangeState(IEnemyState newState)
    {
        if (newState == null || newState == CurrentState)
        {
            return;
        }

        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        CurrentState = newState;

        Debug.Log($"{gameObject.name} transitioned to: {newState.GetType().Name}", this);
        CurrentState.Enter();
    }

    public void SelectAttack(AttackData attack)
    {
        SelectedAttack = attack;
    }

    public void StartAttackCooldown(float duration)
    {
        attackCooldownTimer = duration;
    }

    private void HandleGuardBroken()
    {
        ChangeState(StaggeredState);
    }

    private void HandleGuardRecovered()
    {
        ChangeState(IdleState);
    }

    private void OnDrawGizmosSelected()
    {
        if (enemy == null || enemy.Data == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemy.Data.meleeAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemy.Data.rangedAttackRange);
    }
}