using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Combo Asset Blueprint Tree")]
    [SerializeField] private AttackData entryMeleeAttackData;
    [SerializeField] private AttackData entryFlintlockAttackData;

    [Header("Combo Timings")]
    [SerializeField] private float comboResetWindow = 1.0f;

    [Header("Input Buffer")]
    [SerializeField] private float inputBufferWindow = 0.15f;

    [Header("References")]
    [SerializeField] private MeleeAttack meleeAttack;
    [SerializeField] private FlintlockCarousel flintlockCarousel;
    [SerializeField] private Rigidbody2D rb;

    private ComboStateMachine stateMachine = new ComboStateMachine();
    private InputBuffer<AttackType> inputBuffer;
    private IAimProvider aimProvider;
    private Coroutine attackRoutine;

    private AttackData currentAttackData;
    private float comboDecayTimer;

    public int CurrentComboStep => stateMachine.CurrentStep;
    public bool IsAttacking => stateMachine.IsAttacking;
    public bool IsInRecover => stateMachine.CanCancel;

    private void Awake()
    {
        aimProvider = GetComponent<IAimProvider>();
        inputBuffer = new InputBuffer<AttackType>(inputBufferWindow);

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
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
        if (value.isPressed)
        {
            HandleInput(AttackType.Melee);
        }
    }

    public void OnFlintlockInput(InputValue value)
    {
        if (value.isPressed)
        {
            HandleInput(AttackType.Flintlock);
        }
    }

    #endregion

    private void HandleInput(AttackType input)
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

    private void ExecuteStep(AttackType input)
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

    private AttackData EvaluateNextAttackNode(AttackType input)
    {
        if (currentAttackData == null || stateMachine.CurrentStep == 0)
        {
            if (input == AttackType.Melee)
            {
                return entryMeleeAttackData;
            }
            else
            {
                return entryFlintlockAttackData;
            }
        }

        AttackData targetNode;

        if (input == AttackType.Melee)
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

    private IEnumerator PerformAttack(AttackType inputType, AttackData attackData, Vector2 aimDirection)
    {
        if (rb != null && attackData.lungeForce > 0f)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(aimDirection.normalized * attackData.lungeForce, ForceMode2D.Impulse);
        }

        // Dispatch attack execution to either melee or flintlock
        if (inputType == AttackType.Melee && meleeAttack != null)
        {
            meleeAttack.ExecuteSlash(attackData, aimDirection);
        }
        else if (inputType == AttackType.Flintlock && flintlockCarousel != null)
        {
            flintlockCarousel.FireComboShot(attackData, aimDirection);
        }

        // Active windup / Hitframe
        yield return new WaitForSeconds(attackData.activeHitDuration);

        // Enter Recovery
        stateMachine.EnterRecovery();

        // Check input buffer upon entering recovery
        if (inputBuffer.HasBufferedInput)
        {
            AttackType nextInput = inputBuffer.Consume().Value;
            ExecuteStep(nextInput);
            yield break;
        }

        // Recovery Window
        yield return new WaitForSeconds(attackData.recoveryDuration);

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