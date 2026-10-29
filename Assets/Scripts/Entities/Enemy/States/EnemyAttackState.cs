using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private readonly InfantryAI ai;
    private readonly TargetDetector detector;
    private readonly EntityMotor motor;
    private bool isTimelineFinished;

    public EnemyAttackState(InfantryAI ai, TargetDetector detector, EntityMotor motor)
    {
        this.ai = ai;
        this.detector = detector;
        this.motor = motor;
    }

    public void Enter()
    {
        isTimelineFinished = false;

        if (ai.SelectedAttack == null)
        {
            ai.ChangeState(ai.ChaseState);
            return;
        }

        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }

        if (ai.AttackExecutor != null)
        {
            ai.AttackExecutor.ExecuteAttack(ai.SelectedAttack, OnTimelineFinished);
        }
        else
        {
            OnTimelineFinished();
        }
    }

    public void UpdateState()
    {
        if (isTimelineFinished)
        {
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

            if (ai.SelectedAttack != null && ai.SelectedAttack.nextMeleeFollowUp != null)
            {
                ai.SelectAttack(ai.SelectedAttack.nextMeleeFollowUp);
                Enter();
                return;
            }

            ai.ChangeState(ai.ChaseState);
        }
    }

    public void FixedUpdateState() { }

    public void Exit() 
    {
        isTimelineFinished = false;
    }

    private void OnTimelineFinished()
    {
        isTimelineFinished = true;
    }
}
