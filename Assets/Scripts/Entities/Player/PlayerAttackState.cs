using UnityEngine;

public class PlayerAttackState : IPlayerState
{
    private readonly PlayerController player;
    private readonly PlayerAttack playerAttack;

    private bool isTimelineFinished;
    private PlayerActionType pendingInput = PlayerActionType.None;

    public PlayerAttackState(PlayerController player, PlayerAttack playerAttack)
    {
        this.player = player;
        this.playerAttack = playerAttack;
    }

    public void Enter()
    {
        isTimelineFinished = false;
        pendingInput = PlayerActionType.None;

        if (playerAttack.IsFreeFireStance)
        {
            Vector2 aimDirection;
            if (player.AimProvider != null)
            {
                aimDirection = player.AimProvider.AimDirection;
            }
            else
            {
                aimDirection = Vector2.right;
            }

            playerAttack.StateMachine.StartStep();
            playerAttack.AttackExecutor.ExecuteFreeFire(
                aimDirection,
                playerAttack.FreeFireAttackData,
                OnFreeFireFinished
            );

            return;
        }

        if (playerAttack.HasBufferedInput(out PlayerActionType actionType))
        {
            StartAttackStep(actionType);
        }
    }

    public void StartAttackStep(PlayerActionType inputType)
    {
        isTimelineFinished = false;

        bool isDashing = (player.PreviousState == player.DashState);
        AttackData targetAttack = null;

        if (playerAttack.CurrentComboStep == 0 && isDashing && inputType == PlayerActionType.Melee && playerAttack.DashMeleeAttackData != null)
        {
            targetAttack = playerAttack.DashMeleeAttackData;
        }
        else
        {
            targetAttack = playerAttack.SequenceEvaluator.EvaluateNextNode(
                inputType,
                playerAttack.CurrentAttackData,
                playerAttack.CurrentComboStep
            );
        }

        if (targetAttack == null)
        {
            playerAttack.ResetCombo();
            player.ChangeState(player.IdleMoveState);
            return;
        }

        playerAttack.CurrentAttackData = targetAttack;
        playerAttack.StateMachine.StartStep();
        playerAttack.RefreshComboDecayTimer();

        playerAttack.AttackExecutor.ExecuteAttack(
            inputType,
            targetAttack,
            playerAttack.CurrentComboStep,
            OnTimelineFinished
        );
    }

    public void UpdateState()
    {
        if (playerAttack.IsFreeFireStance)
        {
            return;
        }
        if (playerAttack.HasBufferedInput(out PlayerActionType bufferedInput))
        {
            if (bufferedInput == PlayerActionType.Dash && player.CanDash())
            {
                playerAttack.StateMachine.CompleteAttack();
                player.ChangeState(player.DashState);
                return;
            }

            pendingInput = bufferedInput;
        }

        if (isTimelineFinished)
        {
            if (pendingInput == PlayerActionType.Melee || pendingInput == PlayerActionType.Flintlock)
            {
                PlayerActionType next = pendingInput;
                pendingInput = PlayerActionType.None;
                StartAttackStep(next);
                return;
            }

            bool hasFollowup = (playerAttack.CurrentAttackData != null) &&
                               (playerAttack.CurrentAttackData.nextMeleeFollowUp != null ||
                                playerAttack.CurrentAttackData.nextFlintlockFollowUp != null);
            
            if (!hasFollowup)
            {
                playerAttack.ResetCombo();
            }
            else
            {
                playerAttack.StateMachine.CompleteAttack();
            }

            player.ChangeState(player.IdleMoveState);
        }
    }

    public void FixedUpdateState()
    {

    }

    public void Exit()
    {
        isTimelineFinished = false;
        pendingInput = PlayerActionType.None;
    }

    private void OnTimelineFinished()
    {
        isTimelineFinished = true;
    }

    private void OnFreeFireFinished()
    {
        playerAttack.ResetCombo();
        player.ChangeState(player.IdleMoveState);
    }
}
