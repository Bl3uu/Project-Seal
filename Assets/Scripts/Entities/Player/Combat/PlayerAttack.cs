using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EntityMotor))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Asset Blueprint Tree")]
    [SerializeField] private AttackData entryMeleeAttackData;
    [SerializeField] private AttackData entryFlintlockAttackData;
    [SerializeField] private AttackData dashMeleeAttackData;

    [Header("Combo Timings")]
    [SerializeField] private float comboResetWindow = 1.0f;
    [SerializeField] private float inputBufferWindow = 0.15f;

    [Header("Free-Fire Settings")]
    [SerializeField] private AttackData freeFireAttackData;
    [SerializeField] private bool isFreeFireStance = false;

    private ComboStateMachine stateMachine = new ComboStateMachine();
    private PlayerInputBuffer<PlayerActionType> inputBuffer;
    private ComboSequenceEvaluator sequenceEvaluator;
    private AttackExecutor attackExecutor;
    private FlintlockCarousel flintlockCarousel;
    private IAimProvider aimProvider;
    private PlayerDash playerDash;

    private AttackData currentAttackData;
    private float comboDecayTimer;

    public int CurrentComboStep => stateMachine.CurrentStep;
    public bool IsAttacking => stateMachine.IsAttacking;
    public bool IsInRecover => stateMachine.CanCancel;
    public bool IsFreeFireStance => isFreeFireStance;

    private void Awake()
    {
        attackExecutor = GetComponent<AttackExecutor>();
        flintlockCarousel = GetComponentInChildren<FlintlockCarousel>();
        inputBuffer = new PlayerInputBuffer<PlayerActionType>(inputBufferWindow);
        aimProvider = GetComponent<IAimProvider>();
        playerDash = GetComponent<PlayerDash>();
        sequenceEvaluator = new ComboSequenceEvaluator(entryMeleeAttackData, entryFlintlockAttackData);
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
        Debug.Log($"[PlayAttack] Swtiched Stance. Free-Fire Active: {isFreeFireStance}");
    }

    public void OnFreeFireInput(InputValue value)
    {
        if (!value.isPressed || flintlockCarousel == null || !isFreeFireStance)
        {
            return;
        }

        if (stateMachine.IsAttacking)
        {
            return;
        }

        stateMachine.StartStep();

        Vector2 aimDirection;

        if (aimProvider != null)
        {
            aimDirection = aimProvider.AimDirection;
        }
        else
        {
            aimDirection = Vector2.right;
        }

        attackExecutor.ExecuteFreeFire(aimDirection, freeFireAttackData, this);
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
            bool isDashing = (playerDash != null && playerDash.IsDashing);

            if (stateMachine.CurrentStep == 0 && isDashing && input == PlayerActionType.Melee && dashMeleeAttackData != null)
            {
                Debug.Log("Executing Dash Attack");
                ExecuteDashAttack();
            }
            else
            {
                ExecuteStep(input);
            }
        }
        else
        {
            inputBuffer.Buffer(input);
        }
    }

    private void ExecuteStep(PlayerActionType input)
    {
        AttackData targetAttack = sequenceEvaluator.EvaluateNextNode(input, currentAttackData, stateMachine.CurrentStep);

        if (targetAttack == null)
        {
            Debug.LogWarning("[PlayAttack] No valid AttackData node found for input. Resetting combo.");
            ResetCombo();
            return;
        }

        attackExecutor.StopActiveAttack();

        currentAttackData = targetAttack;
        stateMachine.StartStep();
        comboDecayTimer = comboResetWindow;

        attackExecutor.ExecuteAttack(input, currentAttackData, stateMachine.CurrentStep, this);
    }

    private void ExecuteDashAttack()
    {
        attackExecutor.StopActiveAttack();
        
        currentAttackData = dashMeleeAttackData;
        stateMachine.StartStep();
        comboDecayTimer = comboResetWindow;

        attackExecutor.ExecuteAttack(PlayerActionType.Melee, currentAttackData, stateMachine.CurrentStep, this);
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

    public void NotifyActionInterrupt()
    {
        if (stateMachine.CurrentStep == 0)
        {
            return;
        }

        if (attackExecutor != null)
        {
            attackExecutor.StopActiveAttack();
        }

        inputBuffer.Clear();
        stateMachine.CompleteAttack();
        comboDecayTimer = comboResetWindow;

        Debug.Log($"[PlayAttack] Action Interrupted! Combo preserved at Step {stateMachine.CurrentStep}.");
    }

    public bool HasBufferedInput(out PlayerActionType bufferedType)
    {
        if (inputBuffer.HasBufferedInput)
        {
            bufferedType = inputBuffer.Consume().Value;
            return (bufferedType == PlayerActionType.Melee || 
                    bufferedType == PlayerActionType.Flintlock ||
                    bufferedType == PlayerActionType.Dash);
        }

        bufferedType = PlayerActionType.None;
        return false;
    }

    public void ExecuteBufferedStep(PlayerActionType nextInput)
    {
        ExecuteStep(nextInput);
    }

    public void BufferInputFromDash(PlayerActionType type)
    {
        inputBuffer.Buffer(type);
    }

    public void CheckForComboCompletion()
    {
        bool hasFollowup = (currentAttackData.nextMeleeFollowUp != null || currentAttackData.nextFlintlockFollowUp != null);
        
        if (!hasFollowup)
        {
            ResetCombo();
        }
        else
        {
            stateMachine.CompleteAttack();
        }
    }

    public void ResetCombo()
    {
        attackExecutor.StopActiveAttack();
        stateMachine.Reset();
        inputBuffer.Clear();
        comboDecayTimer = 0f;
        currentAttackData = null;
        Debug.Log("[PlayAttack] Combo Sequence Reset.");
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