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
    private PlayerDash playerDash;
    private IAimProvider aimProvider;

    private Coroutine attackRoutine;
    private AttackData currentAttackData;

    public bool IsExecuting => attackRoutine != null;

    private void Awake()
    {
        playerMotor = GetComponent<EntityMotor>();
        playerAnimator = GetComponent<EntityAnimator2D>();
        aimProvider = GetComponent<IAimProvider>();
        playerDash = GetComponent<PlayerDash>();

        if (meleeAttack == null)
        {
            meleeAttack = GetComponentInChildren<MeleeAttack>();
        }
        if (flintlockCarousel == null)
        {
            flintlockCarousel = GetComponentInChildren<FlintlockCarousel>();
        }
    }

    public void ExecuteAttack(PlayerActionType inputType, AttackData attackData, int currenStep, PlayerAttack playerAttack)
    {
        StopActiveAttack();
        attackRoutine = StartCoroutine(AttackTimelineRoutine(inputType, attackData, currenStep, playerAttack));
    }

    public void ExecuteFreeFire(Vector2 aimDirection, AttackData freeFireData, PlayerAttack playerAttack)
    {
        StopActiveAttack();
        attackRoutine = StartCoroutine(FreeFireRoutine(aimDirection, freeFireData, playerAttack));
    }

    private IEnumerator AttackTimelineRoutine(PlayerActionType inputType, AttackData attackData, int currenStep, PlayerAttack playerAttack)
    {
        currentAttackData = attackData;

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
                playerAttack.ResetCombo();
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

        bool bufferedNextStep = false;
        PlayerActionType nextInput = PlayerActionType.None;

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

            if (!bufferedNextStep)
            {
                bufferedNextStep = CheckAndCaptureBuffer(playerAttack, ref nextInput);
            }

            if (bufferedNextStep && nextInput == PlayerActionType.Dash && timer > attackData.startupDuration)
            {
                break;
            }

            yield return new WaitForFixedUpdate();
        }

        if (playerMotor != null)
        {
            playerMotor.Move(Vector2.zero, 0f);
        }

        attackRoutine = null;

        if (bufferedNextStep)
        {
            if (nextInput == PlayerActionType.Dash)
            {
                playerAttack.NotifyActionInterrupt();

                if (playerDash != null)
                {
                    playerDash.TriggerDashFromAttack();
                }
            }
            else
            {
                playerAttack.ExecuteBufferedStep(nextInput);
            }
        }
        else
        {
            playerAttack.CheckForComboCompletion();
        }
    }

    private IEnumerator FreeFireRoutine(Vector2 aimDirection, AttackData freeFireData, PlayerAttack playerAttack)
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
        playerAttack.ResetCombo();
    }

    private bool CheckAndCaptureBuffer(PlayerAttack playerAttack, ref PlayerActionType nextInput)
    {
        if (playerAttack.HasBufferedInput(out PlayerActionType bufferedType))
        {
            if (bufferedType == PlayerActionType.Melee ||
                bufferedType == PlayerActionType.Flintlock ||
                bufferedType == PlayerActionType.Dash)
            {
                nextInput = bufferedType;
                return true;
            }
        }

        return false;
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
