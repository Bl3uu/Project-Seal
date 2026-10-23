using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private readonly InfantryAI ai;
    private readonly TargetDetector detector;
    private readonly EnemyAim aim;
    private readonly EntityMotor motor;
    private readonly EnemyData data;

    private float strafeDirection = 1f;
    private float strafeTimer;
    private bool isStrafing = false;

    private const float SPACING_BUFFER = 0.4f;

    public EnemyChaseState(InfantryAI ai, TargetDetector detector, EnemyAim aim, EntityMotor motor, EnemyData data)
    {
        this.ai = ai;
        this.detector = detector;
        this.aim = aim;
        this.motor = motor;
        this.data = data;
    }

    public void Enter() 
    {
        if (Random.value > 0.5f) 
        {
            strafeDirection = 1f;
        }
        else
        {
            strafeDirection = -1f;
        }

        strafeTimer = Random.Range(1f, 2.5f);
        isStrafing = false;
    }

    public void UpdateState()
    {
        Transform target = detector.CurrentTarget;
        if (!detector.HasTarget || data == null)
        {
            ai.ChangeState(ai.IdleState);
            return;
        }

        Vector2 direction = detector.DirectionToTarget;
        float distance = detector.DistanceToTarget;

        if (aim != null)
        {
            aim.UpdateAim(direction);
        }

        if (ai.CanAttack)
        {
            if (distance <= data.meleeAttackRange && data.meleeAttackData.Length > 0)
            {
                // pick random melee attack from melee attack list
                int randomIndex = Random.Range(0, data.meleeAttackData.Length);
                ai.SelectAttack(data.meleeAttackData[randomIndex]);
                ai.ChangeState(ai.AttackState);
                return;
            }
            else if (distance <= data.rangedAttackRange && data.rangeAttackData != null)
            {
                ai.SelectAttack(data.rangeAttackData);
                ai.ChangeState(ai.AttackState);
                return;
            }
        }

        float innerThreshold = data.meleeAttackRange;
        float outerThreshold = data.meleeAttackRange + SPACING_BUFFER;

        if (!isStrafing && distance <= innerThreshold)
        {
            isStrafing = true;
        }
        else if (isStrafing && distance > outerThreshold) 
        {
            isStrafing = false;
        }

        if (motor != null)
        {
            if (!isStrafing)
            {
                motor.Move(direction, data.chaseSpeed);
            }
            else
            {
                strafeTimer -= Time.deltaTime;
                if (strafeTimer <= 0f)
                {
                    strafeDirection *= -1f;
                    strafeTimer = Random.Range(1f, 2.5f);
                }

                Vector2 perpendicularDirection = new Vector2(-direction.y, direction.x) * strafeDirection;

                motor.Move(perpendicularDirection.normalized, data.chaseSpeed * 0.5f);
            }
            
        }
    }

    public void Exit() 
    {
        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }
    }
}
