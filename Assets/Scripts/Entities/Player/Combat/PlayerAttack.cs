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

    private AttackData currentAttackData;
    private float comboDecayTimer;

    public ComboStateMachine StateMachine => stateMachine;
    public PlayerInputBuffer<PlayerActionType> InputBuffer => inputBuffer;
    public ComboSequenceEvaluator SequenceEvaluator => sequenceEvaluator;
    public AttackExecutor AttackExecutor => attackExecutor;
    public FlintlockCarousel FlintlockCarousel => flintlockCarousel;
    public AttackData DashMeleeAttackData => dashMeleeAttackData;
    public AttackData FreeFireAttackData => freeFireAttackData;

    public AttackData CurrentAttackData
    {
        get => currentAttackData;
        set => currentAttackData = value;
    }

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

    public void ToggleStance()
    {
        if (stateMachine.IsAttacking)
        {
            return;
        }

        isFreeFireStance = !isFreeFireStance;
        Debug.Log($"[PlayAttack] Swtiched Stance. Free-Fire Active: {isFreeFireStance}");
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

    public void ResetCombo()
    {
        if (attackExecutor != null)
        {
            attackExecutor.StopActiveAttack();
        }
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

    public void RefreshComboDecayTimer()
    {
        comboDecayTimer = comboResetWindow;
    }
}