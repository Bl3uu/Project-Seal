using System.Collections.Generic;
using UnityEngine;

public class InfantryAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemy;
    [SerializeField] private EnemyAttackExecutor attackExecutor;
    [SerializeField] private TargetDetector targetDetector;
    [SerializeField] private EnemyAim enemyAim;
    [SerializeField] private EntityMotor motor;
    [SerializeField] private Poise poise;

    [Header("AI Decision Components")]
    [SerializeField] private DecisionEvaluator decisionEvaluator;
    [SerializeField] private AmmoTracker ammoTracker;

    // State Instances
    public EnemyIdleState IdleState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }

    // Attack Properties
    public AttackData SelectedAttack { get; private set;  }
    public bool HasBullet { get; private set; } = true;
    public Enemy Enemy => enemy;
    public EnemyData Data => data;
    public EnemyAttackExecutor AttackExecutor => attackExecutor;
    public DecisionEvaluator DecisionEvaluator => decisionEvaluator;
    public AmmoTracker AmmoTracker => ammoTracker;
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
        if (attackExecutor == null)
        {
            attackExecutor = GetComponent<EnemyAttackExecutor>();
        }
        if (decisionEvaluator == null)
        {
            decisionEvaluator = GetComponent<DecisionEvaluator>();
        }
        if (ammoTracker == null)
        {
            ammoTracker = GetComponent<AmmoTracker>();
        }

        // Attack property handling
        if (enemy != null)
        {
            data = enemy.Data;
        }

        IdleState = new EnemyIdleState(this, targetDetector);
        ChaseState = new EnemyChaseState(this, targetDetector, enemyAim, motor, data, decisionEvaluator);
        AttackState = new EnemyAttackState(this, targetDetector, motor);
    }

    private void Start()
    {
        if (enemy != null)
        {
            enemy.ChangeState(IdleState);
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

        if (enemy != null)
        {
            if (enemy.CurrentState != null)
            {
                enemy.CurrentState.UpdateState();
            }
            else
            {
                if (targetDetector.HasTarget)
                {
                    enemy.ChangeState(ChaseState);
                }
                else
                {
                    enemy.ChangeState(IdleState);
                }
            }
        }
    }

    public void SelectAttack(AttackData attack)
    {
        SelectedAttack = attack;
    }

    public void StartAttackCooldown(float duration)
    {
        attackCooldownTimer = duration;

        Debug.Log($"[InfantryAI] Cooldown started: {duration}s", this);
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