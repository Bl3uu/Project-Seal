using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class AttackExecutor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private FlintlockCarousel flintlockCarousel;

    private EntityMotor playerMotor;
    private EntityAnimator2D playerAnimator;
    private IAimProvider aimProvider;

    private Coroutine attackRoutine;

    private void Awake()
    {
        playerMotor = GetComponent<EntityMotor>();
        playerAnimator = GetComponent<EntityAnimator2D>();
        aimProvider = GetComponent<IAimProvider>();

        if (meleeAttack == null)
        {
            meleeAttack = GetComponentInChildren<MeleeAttack>();
        }
        if (flintlockCarousel == null)
        {
            flintlockCarousel = GetComponentInChildren<FlintlockCarousel>();
        }
    }

    public void ExecuteAttack(PlayerActionType inputType, AttackData attackData, int currenStep, Action onTimelineFinished)
    {
        StopActiveAttack();
        attackRoutine = StartCoroutine(AttackTimelineRoutine(inputType, attackData, currenStep, onTimelineFinished));
    }

    public void ExecuteFreeFire(Vector2 aimDirection, AttackData freeFireData, Action onFinished)
    {
        StopActiveAttack();
        attackRoutine = StartCoroutine(FreeFireRoutine(aimDirection, freeFireData, onFinished));
    }

    private IEnumerator AttackTimelineRoutine(PlayerActionType inputType, AttackData attackData, int currenStep, Action onTimelineFinished)
    {
        Vector2 aimDirection;
        if (aimProvider != null)
        {
            aimDirection = aimProvider.AimDirection;
        }
        else
        {
            aimDirection = Vector2.right;
        }

        if (inputType == PlayerActionType.Flintlock && flintlockCarousel != null)
        {
            bool shotFired = flintlockCarousel.FireComboShot(attackData, aimDirection);
            if (!shotFired)
            {
                attackRoutine = null;
                if (onTimelineFinished != null)
                {
                    onTimelineFinished();
                }
                yield break;
            }
        }

        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        if (playerAnimator != null)
        {
            playerAnimator.PlayAttack(currenStep, aimAngle);
        }

        if (inputType == PlayerActionType.Melee && meleeAttack != null)
        {
            meleeAttack.PrepareAttackVFX(attackData, aimDirection);
        }

        float totalAttackDuration = attackData.startupDuration + attackData.activeHitDuration + attackData.recoveryDuration;
        float timer = 0f;

        while (timer < totalAttackDuration)
        {
            timer += Time.fixedDeltaTime;

            if (inputType == PlayerActionType.Melee && attackData.lungeForce > 0f && playerMotor)
            {
                float activeStartTime = attackData.startupDuration;
                float activeEndTime = attackData.startupDuration + attackData.activeHitDuration;
                
                if (timer >= activeStartTime && timer <= activeEndTime)
                {
                    playerMotor.Move(aimDirection, attackData.lungeForce);
                }
                else
                {
                    playerMotor.Move(Vector2.zero, 0f);
                }
            }

            yield return new WaitForFixedUpdate();
        }

        if (playerMotor != null)
        {
            playerMotor.Move(Vector2.zero, 0f);
        }

        attackRoutine = null;
        if (onTimelineFinished != null)
        {
            onTimelineFinished();
        }
    }

    private IEnumerator FreeFireRoutine(Vector2 aimDirection, AttackData freeFireData, Action onFinished)
    {
        if (flintlockCarousel != null)
        {
            flintlockCarousel.FireFreeShot(aimDirection, freeFireData);
        }

        float duration;
        if (freeFireData != null)
        {
            duration = freeFireData.activeHitDuration + freeFireData.recoveryDuration;
        }
        else
        {
            duration = 0.15f;
        }

        yield return new WaitForSeconds(duration); 

        attackRoutine = null;
        if (onFinished != null)
        {
            onFinished();
        }
    }

    public void StopActiveAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (playerMotor != null)
        {
            playerMotor.Move(Vector2.zero, 0f);
        }
    }
}
