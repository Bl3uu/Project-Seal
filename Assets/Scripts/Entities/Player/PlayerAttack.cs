using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ComboInputType
{
    Melee,
    Flintlock
}

public struct ComboStepData
{
    public int StepIndex; // 1, 2, or 3
    public ComboInputType InputType;
    public Vector2 AimDirection;
    public Vector2 AimWorldPosition;
}

public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Timings")]
    [SerializeField] private float comboResetWindow = 1.0f;
    [SerializeField] private float activeHitDuration = 0.1f;
    [SerializeField] private float recoveryDuration = 0.25f;

    [Header("Input Buffer")]
    [SerializeField] private float inputBufferWindow = 0.15f;

    [Header("References")]
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private FlintlockCarousel flintlockCarousel;

    private ComboStateMachine stateMachine = new ComboStateMachine();
    private InputBuffer<ComboInputType> inputBuffer;
    private IAimProvider aimProvider;
    private Coroutine attackRoutine;

    private float comboDecayTimer;

    public int CurrentComboStep => stateMachine.CurrentStep;
    public bool IsAttacking => stateMachine.IsAttacking;
    public bool IsInRecover => stateMachine.CanCancel;

    private void Awake()
    {
        aimProvider = GetComponent<IAimProvider>();
        inputBuffer = new InputBuffer<ComboInputType>(inputBufferWindow);

        if (meleeAttack == null)
        {
            meleeAttack = GetComponentInChildren<MeleeAttack>();
        }
        if (flintlockCarousel == null)
        {
            flintlockCarousel = GetComponentInChildren<FlintlockCarousel>();
        }
    }

    private void Update()
    {
        inputBuffer.Tick(Time.deltaTime);
        UpdateComboDecay();
    }

    #region Input Handlers

    public void OnMeleeInput(InputValue value)
    {
        if (value.isPressed)
        {
            HandleInput(ComboInputType.Melee);
        }
    }

    public void OnFlintlockInput(InputValue value)
    {
        if (value.isPressed)
        {
            HandleInput(ComboInputType.Flintlock);
        }
    }

    #endregion

    private void HandleInput(ComboInputType input)
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

    private void ExecuteStep(ComboInputType input)
    {
        StopActiveRoutine();

        stateMachine.StartStep();
        comboDecayTimer = comboResetWindow;

        ComboStepData stepData = new ComboStepData();
        stepData.StepIndex = stateMachine.CurrentStep;
        stepData.InputType = input;

        if (aimProvider != null)
        {
            stepData.AimDirection = aimProvider.AimDirection;
        }
        else
        {
            stepData.AimDirection = Vector2.zero;
        }

        stepData.AimWorldPosition = (Vector2)transform.position;

        attackRoutine = StartCoroutine(PerformAttack(stepData));
    }

    private IEnumerator PerformAttack(ComboStepData stepData)
    {
        // Dispatch attack execution to either melee or flintlock
        if (stepData.InputType == ComboInputType.Melee && meleeAttack != null)
        {
            meleeAttack.ExecuteSlash(stepData);
        }
        else if (stepData.InputType == ComboInputType.Flintlock && flintlockCarousel != null)
        {
            flintlockCarousel.FireComboShot(stepData);
        }

        // Active windup / Hitframe
        yield return new WaitForSeconds(activeHitDuration);

        // Enter Recovery
        stateMachine.EnterRecovery();

        // Check input buffer upon entering recovery
        if (inputBuffer.HasBufferedInput)
        {
            ComboInputType nextInput = inputBuffer.Consume().Value;
            ExecuteStep(nextInput);
            yield break;
        }

        // Recovery Window
        yield return new WaitForSeconds(recoveryDuration);

        // Attack action finished normally
        if (stateMachine.CurrentStep >= 3)
        {
            ResetCombo();
        }
        else
        {
            stateMachine.CompleteAttack();
        }
    }

    public bool TryCancelAttack()
    {
        if (stateMachine.CanCancel)
        {
            ResetCombo();
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