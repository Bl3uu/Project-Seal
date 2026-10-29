using System;
using System.Collections;
using UnityEngine;

public class EnemyAttackExecutor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private RangeAttack rangeAttack;

    private EntityMotor motor;
    private TargetDetector detector;
    private Coroutine attackRoutine;

    public bool IsExecuting => attackRoutine != null;

    private void Awake()
    {
        motor = GetComponent<EntityMotor>();
        detector = GetComponent<TargetDetector>();
        if (meleeAttack == null)
        {
            meleeAttack = GetComponentInChildren<MeleeAttack>();
        }
        if (rangeAttack == null)
        {
            rangeAttack = GetComponentInChildren<RangeAttack>();
        }
    }

    public void ExecuteAttack(AttackData attackData, Action onTimelineFinished)
    {
        StopActiveAttack();
        attackRoutine = StartCoroutine(AttackTimelineRoutine(attackData, onTimelineFinished));
    }

    public IEnumerator AttackTimelineRoutine(AttackData attackData, Action onTimelineFinished)
    {
        Vector2 attackDirection;
        if (detector != null && detector.HasTarget)
        {
            attackDirection = detector.DirectionToTarget;
        }
        else
        {
            attackDirection = Vector2.right;
        }

        bool isRanged = attackData != null && attackData.attackType == PlayerActionType.Flintlock;

        if (isRanged && rangeAttack != null)
        {
            rangeAttack.Fire(attackDirection, attackData);
        }
        else if (meleeAttack != null)
        {
            meleeAttack.PrepareAttackVFX(attackData, attackDirection);
        }

        float totalDuration = attackData.startupDuration + attackData.activeHitDuration + attackData.recoveryDuration;
        float timer = 0f;

        while (timer < totalDuration)
        {
            timer += Time.fixedDeltaTime;

            if (attackData.lungeForce > 0f && motor != null && !isRanged)
            {
                float activeStartTime = attackData.startupDuration;
                float activeEndTime = attackData.startupDuration + attackData.activeHitDuration;

                if (timer >= activeStartTime && timer <= activeEndTime)
                {
                    motor.Move(attackDirection, attackData.lungeForce);
                }
                else
                {
                    motor.Move(Vector2.zero, 0f);
                }
            }

            yield return new WaitForFixedUpdate();
        }

        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }

        attackRoutine = null;
        if (onTimelineFinished != null)
        {
            onTimelineFinished();
        }
    }

    public void StopActiveAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (motor != null)
        {
            motor.Move(Vector2.zero, 0f);
        }
    }
}
