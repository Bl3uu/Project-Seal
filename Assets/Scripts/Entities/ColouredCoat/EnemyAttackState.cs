using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private readonly InfantryAI ai;
    private readonly TargetDetector detector;
    private readonly EntityMotor motor;
    private float windUpTimer;

    public EnemyAttackState(InfantryAI ai, TargetDetector detector, EntityMotor motor)
    {
        this.ai = ai;
        this.detector = detector;
        this.motor = motor;
    }

    public void Enter()
    {
        if (ai.SelectedAttack != null)
        {
            windUpTimer = ai.SelectedAttack.startupDuration;
        }

        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }
    }

    public void UpdateState()
    {
        if (ai.SelectedAttack == null)
        {
            ai.ChangeState(ai.ChaseState);
            return;
        }

        windUpTimer -= Time.deltaTime;

        if (windUpTimer <= 0f)
        {
            if (ai.SelectedAttack.lungeForce > 0f && motor != null && detector.HasTarget)
            {
                motor.Move(detector.DirectionToTarget, ai.SelectedAttack.lungeForce);
            }

            float cooldown;
            if (ai.Data != null)
            {
                cooldown = ai.Data.globalAttackCooldown;
            }
            else
            {
                cooldown = 1.5f;
            }
            ai.StartAttackCooldown(cooldown);
            ai.ChangeState(ai.ChaseState);
        }
    }

    public void Exit() { }
}
