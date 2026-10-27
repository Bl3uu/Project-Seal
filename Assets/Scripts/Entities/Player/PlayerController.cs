using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    private float nextDashAvailableTime = 0f;
    private PlayerAttack playerAttack;

    public EntityMotor Motor { get; private set; }
    public IAimProvider AimProvider {  get; private set; }
    public IAnimationController AnimationController { get; private set; }

    public Vector2 CurrentInputVector { get; private set; }
    public float MoveSpeed => moveSpeed;
    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;

    public IPlayerState CurrentState {  get; private set; }
    public IPlayerState PreviousState { get; private set; }

    public PlayerIdleMoveState IdleMoveState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerKnockbackState KnockbackState { get; private set; }
    public PlayerAttackState AttackState {  get; private set; }

    private void Awake()
    {
        Motor = GetComponent<EntityMotor>();
        AimProvider = GetComponent<IAimProvider>();
        AnimationController = GetComponent<IAnimationController>();
        playerAttack = GetComponent<PlayerAttack>();

        IdleMoveState = new PlayerIdleMoveState(this);
        DashState = new PlayerDashState(this);
        KnockbackState = new PlayerKnockbackState(this);
        AttackState = new PlayerAttackState(this, playerAttack);
    }

    private void Start()
    {
        ChangeState(IdleMoveState);
    }

    private void Update()
    {
        CurrentInputVector = ReadInputVector();

        if (CurrentState != null)
        {
            CurrentState.UpdateState();
        }
    }

    private void FixedUpdate()
    {
        if (CurrentState != null)
        {
            CurrentState.FixedUpdateState();
        }
    }

    public void ChangeState(IPlayerState newState)
    {
        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        PreviousState = CurrentState;
        CurrentState = newState;

        if (CurrentState != null)
        {
            CurrentState.Enter();
        }
    }

    #region Input Event Handlers

    public void OnMeleeInput(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (playerAttack != null)
        {
            playerAttack.InputBuffer.Buffer(PlayerActionType.Melee);
        }

        if (CurrentState == IdleMoveState || CurrentState == DashState)
        {
            ChangeState(AttackState);
        }
    }

    public void OnFlintlockInput(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (playerAttack != null)
        {
            playerAttack.InputBuffer.Buffer(PlayerActionType.Flintlock);
        }

        if (CurrentState == IdleMoveState || CurrentState == DashState)
        {
            ChangeState(AttackState);
        }
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (CanDash() && CurrentState == IdleMoveState)
        {
            ChangeState(DashState);
        }
        else if (playerAttack != null && playerAttack.StateMachine.IsAttacking)
        {
            playerAttack.InputBuffer.Buffer(PlayerActionType.Dash);
        }
    }

    public void OnFreeFireInput(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (playerAttack != null && playerAttack.IsFreeFireStance && CurrentState == IdleMoveState)
        {
            ChangeState(AttackState);
        }
    }

    public void OnToggleStanceInput(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (playerAttack != null)
        {
            playerAttack.ToggleStance();
        }
    }

    public void OnReloadInput(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        if (playerAttack != null && !playerAttack.IsAttacking && playerAttack.FlintlockCarousel != null)
        {
            playerAttack.FlintlockCarousel.StartReload();
        }
    }

    #endregion

    public bool CanDash()
    {
        return Time.time >= nextDashAvailableTime;
    }

    public void SetDashCooldown()
    {
        nextDashAvailableTime = Time.time + dashCooldown;
    }

    public void ApplyKnockback(float duration)
    {
        KnockbackState.SetDuration(duration);
        ChangeState(KnockbackState);
    }

    private Vector2 ReadInputVector()
    {
        float x = 0f;
        float y = 0f;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
        }

        return new Vector2(x, y).normalized;
    }
}
