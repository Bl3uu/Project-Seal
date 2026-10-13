using System.Collections;
using Unity.AppUI.UI;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EntityMotor))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Asset Blueprint Tree")]
    [SerializeField] private AttackData entryMeleeAttackData;
    [SerializeField] private AttackData entryFlintlockAttackData;

    [Header("Combo Timings")]
    [SerializeField] private float comboResetWindow = 1.0f;
    [SerializeField] private float inputBufferWindow = 0.15f;

    [Header("References")]
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private FlintlockCarousel flintlockCarousel;

    private EntityMotor playerMotor;
    private EntityAnimator2D entityAnimator;

    [Header("Free-Fire Settings")]
    [SerializeField] private AttackData freeFireAttackData;

    [Header("Stance settings")]
    [SerializeField] private bool isFreeFireStance = false;

    private ComboStateMachine stateMachine = new ComboStateMachine();
    private PlayerInputBuffer<PlayerActionType> inputBuffer;
    private IAimProvider aimProvider;
    private Coroutine attackRoutine;

    private AttackData currentAttackData;
    private float comboDecayTimer;

    public int CurrentComboStep => stateMachine.CurrentStep;
    public bool IsAttacking => stateMachine.IsAttacking;
    public bool IsInRecover => stateMachine.CanCancel;
    public bool IsFreeFireStance => isFreeFireStance;

    private void Awake()
    {
        playerMotor = GetComponent<EntityMotor>();
        aimProvider = GetComponent<IAimProvider>();
        entityAnimator = GetComponent<EntityAnimator2D>();
        inputBuffer = new PlayerInputBuffer<PlayerActionType>(inputBufferWindow);

        if (meleeAttack == null)
        {
            meleeAttack = GetComponentInChildren<MeleeAttack>();
        }
        if (flintlockCarousel == null)
        {
            flintlockCarousel = GetComponentInChildren<FlintlockCarousel>();
        }
    }

    private void Start()
    {
        ResetCombo();
    }

    private void Update()
    {
        inputBuffer.Tick(Time.deltaTime);
        UpdateComboDecay();
    }

    #region Input Handlers

    public void OnMeleeInput(InputValue value)
    {
        if (!value.isPressed || isFreeFireStance)
        {
            return;
        }

        HandleInput(PlayerActionType.Melee);
    }

    public void OnFlintlockInput(InputValue value)
    {
        if (!value.isPressed || isFreeFireStance)
        {
            return;
        }

        HandleInput(PlayerActionType.Flintlock);
    }

    public void OnToggleStanceInput(InputValue value)
    {
        if (!value.isPressed || stateMachine.IsAttacking) 
        {
            return;
        }

        isFreeFireStance = !isFreeFireStance;
        Debug.Log($"[PlayerAttack] Swtiched Stance. Free-Fire Active: {isFreeFireStance}");
    }

    public void OnFreeFireInput(InputValue value)
    {
        if (!value.isPressed || flintlockCarousel == null || !isFreeFireStance)
        {
            return;
        }

        if (stateMachine.IsAttacking && !TryCancelAttack())
        {
            return;
        }

        Vector2 aimDirection;

        if (aimProvider != null)
        {
            aimDirection = aimProvider.AimDirection;
        }
        else
        {
            aimDirection = Vector2.right;
        }

        StopActiveRoutine();
        attackRoutine = StartCoroutine(PerformFreeFireShot(aimDirection));
    }

    public void OnReloadInput(InputValue value)
    {
        if (!value.isPressed || flintlockCarousel == null || stateMachine.IsAttacking)
        {
            return;
        }

        flintlockCarousel.StartReload();
    }

    #endregion

    private void HandleInput(PlayerActionType input)
    {
        if (!stateMachine.IsAttacking)
        {
            ExecuteStep(input);
        }
        else
        {
            inputBuffer.Buffer(input);
        }
    }

    private void ExecuteStep(PlayerActionType input)
    {
        AttackData targetAttack = EvaluateNextAttackNode(input);

        if (targetAttack == null)
        {
            Debug.LogWarning("[PlayerAttack] No valid AttackData node found for input. Resetting combo.");
            ResetCombo();
            return;
        }

        StopActiveRoutine();

        currentAttackData = targetAttack;
        stateMachine.StartStep();
        comboDecayTimer = comboResetWindow;

        Vector2 aimDirection;

        if (aimProvider != null)
        {
            aimDirection = aimProvider.AimDirection;
        }
        else
        {
            aimDirection = Vector2.right;
        }

        attackRoutine = StartCoroutine(PerformAttack(input, currentAttackData, aimDirection));
    }

    private AttackData EvaluateNextAttackNode(PlayerActionType input)
    {
        if (currentAttackData == null || stateMachine.CurrentStep == 0)
        {
            if (input == PlayerActionType.Melee)
            {
                return entryMeleeAttackData;
            }
            else
            {
                return entryFlintlockAttackData;
            }
        }

        AttackData targetNode;

        if (input == PlayerActionType.Melee)
        {
            targetNode = currentAttackData.nextMeleeFollowUp;
        }
        else
        {
            targetNode = currentAttackData.nextFlintlockFollowUp;
        }

        if (targetNode != null && targetNode.attackType != input)
        {
            Debug.LogError($"[PlayerAttack] Mismatched AttackType on node '{targetNode.name}'! Expected {input}, but node is typed as {targetNode.attackType}. Blocking sequence step.");
            return null;
        }

        return targetNode;
    }

    private IEnumerator PerformAttack(PlayerActionType inputType, AttackData attackData, Vector2 aimDirection)
    {
        if (inputType == PlayerActionType.Flintlock && flintlockCarousel != null)
        {
            bool shotFired = flintlockCarousel.FireComboShot(attackData, aimDirection);
            if (!shotFired)
            {
                ResetCombo();
                yield break;
            }
        }

        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        if (entityAnimator != null)
        {
            entityAnimator.PlayerAttack(stateMachine.CurrentStep, aimAngle);
        }

        // Wind-Up Phase
        float startupTimer = 0f;
        while (startupTimer < attackData.startupDuration)
        {
            startupTimer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
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

            if (inputType == PlayerActionType.Melee && attackData.lungeForce > 0f)
            {
                playerMotor.Move(aimDirection, attackData.lungeForce);
            }

            yield return new WaitForFixedUpdate();
        }

        playerMotor.Move(Vector2.zero, 0f);

        // Recovery Phase
        stateMachine.EnterRecovery();

        float recoveryTimer = 0f;
        bool bufferedNextStep = false;
        PlayerActionType nextInput = PlayerActionType.None;

        while (recoveryTimer < attackData.recoveryDuration)
        {
            recoveryTimer += Time.fixedDeltaTime;
            if (inputBuffer.HasBufferedInput)
            {
                nextInput = inputBuffer.Consume().Value;
                if (nextInput == PlayerActionType.Melee || nextInput == PlayerActionType.Flintlock)
                {
                    bufferedNextStep = true;
                    break;
                }
            }

            yield return null;
        }

        if (bufferedNextStep)
        {
            ExecuteStep(nextInput);
            yield break;
        }

        bool hasFollowUp = (currentAttackData.nextMeleeFollowUp != null || currentAttackData.nextFlintlockFollowUp != null);

        // Attack action finished normally
        if (!hasFollowUp)
        {
            ResetCombo();
        }
        else
        {
            stateMachine.CompleteAttack();
        }
    }

    private IEnumerator PerformFreeFireShot(Vector2 aimDirection)
    {
        stateMachine.StartStep();

        flintlockCarousel.FireFreeShot(aimDirection, freeFireAttackData);

        float duration;

        if (freeFireAttackData != null)
        {
            duration = freeFireAttackData.activeHitDuration + freeFireAttackData.recoveryDuration;
        }
        else
        {
            duration = 0.15f;
        }

        yield return new WaitForSeconds(duration);

        stateMachine.CompleteAttack();
        ResetCombo();
    }

    public void NotifyActionInterrupt()
    {
        if (stateMachine.CurrentStep == 0)
        {
            return;
        }

        StopActiveRoutine();
        inputBuffer.Clear();
        stateMachine.CompleteAttack();
        comboDecayTimer = comboResetWindow;

        Debug.Log($"[PlayerAttack] Action Interrupted! Combo preserved at Step {stateMachine.CurrentStep}.");
    }

    public bool TryCancelAttack()
    {
        if (stateMachine.CanCancel)
        {
            NotifyActionInterrupt();
            return true;
        }

        return false;
    }

    public void ResetCombo()
    {
        StopActiveRoutine();
        stateMachine.Reset();
        inputBuffer.Clear();
        comboDecayTimer = 0f;
        currentAttackData = null;
        Debug.Log("[PlayerAttack] Combo Sequence Reset.");
    }

    private void StopActiveRoutine()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private void UpdateComboDecay()
    {
        // Combo Step Window Decay
        if (stateMachine.CurrentStep > 0 && !stateMachine.IsAttacking)
        {
            comboDecayTimer -= Time.deltaTime;
            if (comboDecayTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }
}