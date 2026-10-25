using UnityEngine;

[RequireComponent(typeof(EntityMotor))]
public class PlayerDashState : IPlayerState
{
    private readonly PlayerController player;

    private float dashTimer;
    private Vector2 dashDirection;

    public PlayerDashState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter()
    {
        dashTimer = 0f;

        dashDirection = player.CurrentInputVector;

        if (dashDirection == Vector2.zero)
        {
            if (player.AimProvider != null)
            {
                dashDirection = player.AimProvider.AimDirection;
            }
            
            if (dashDirection == Vector2.zero)
            {
                dashDirection = Vector2.down;
            }
        }

        dashDirection = dashDirection.normalized;

        if (player.AnimationController != null)
        {
            player.AnimationController.PlayDash(dashDirection);
        }

        player.SetDashCooldown();
    }

    public void UpdateState()
    {
        dashTimer += Time.deltaTime;

        if (dashTimer >= player.DashDuration)
        {
            player.ChangeState(player.IdleMoveState);
        }
    }

    public void FixedUpdateState()
    {
        if (player.Motor != null)
        {
            player.Motor.Move(dashDirection, player.DashSpeed);
        }
    }

    public void Exit()
    {
        if (player.Motor != null)
        {
            player.Motor.Move(Vector2.zero, 0f);
        }
    }
}
