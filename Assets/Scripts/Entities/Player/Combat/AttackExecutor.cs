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
            playerAnimator.PlayerAttack(currenStep, aimAngle);
        }

        bool bufferedNextStep = false;
        PlayerActionType nextInput = PlayerActionType.None;

        // Startup / Wind-up Phase
        if (attackData.startupDuration > 0f)
        {
            float startupTimer = 0f;
            while (startupTimer < attackData.startupDuration)
            {
                startupTimer += Time.fixedDeltaTime;
                
                if (!bufferedNextStep)
                {
                    bufferedNextStep = CheckAndCaptureBuffer(playerAttack, ref nextInput);
                }

                yield return new WaitForFixedUpdate();
            }
        }

        // Active Hit Phase
        if (inputType == PlayerActionType.Melee && meleeAttack != null)
        {
            meleeAttack.ExecuteSlash(attackData, aimDirection);
        }

        float activeTimer = 0f;
        while (activeTimer < attackData.activeHitDuration)
        {
            activeTimer += Time.fixedDeltaTime;

            if (inputType == PlayerActionType.Melee && attackData.lungeForce > 0f && playerMotor != null)
            {
                playerMotor.Move(aimDirection, attackData.lungeForce);
            }

            if (!bufferedNextStep)
            {
                bufferedNextStep = CheckAndCaptureBuffer(playerAttack, ref nextInput);
            }

            yield return new WaitForFixedUpdate();
        }

        if (playerMotor != null)
        {
            playerMotor.Move(Vector2.zero, 0f);
        }

        // Recovery Phase
        float recoveryTimer = 0f;

        while (recoveryTimer < attackData.recoveryDuration)
        {
            recoveryTimer += Time.deltaTime;

            if (!bufferedNextStep)
            {
                bufferedNextStep = CheckAndCaptureBuffer(playerAttack, ref nextInput);
            }

            if (bufferedNextStep)
            {
                break;
            }

            yield return null;
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
