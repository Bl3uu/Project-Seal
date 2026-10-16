using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private readonly InfantryAI ai;
    private readonly TargetDetector detector;
    private readonly EnemyAim aim;
    private readonly EntityMotor motor;
    private readonly EnemyData data;

    public EnemyChaseState(InfantryAI ai, TargetDetector detector, EnemyAim aim, EntityMotor motor, EnemyData data)
    {
        this.ai = ai;
        this.detector = detector;
        this.aim = aim;
        this.motor = motor;
        this.data = data;
    }

    public void Enter() { }

    public void UpdateState()
    {
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

        if (motor != null)
        {
            motor.Move(direction, data.chaseSpeed);
        }
    }

    public void Exit() { }
}
