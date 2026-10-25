using UnityEngine;

public class PlayerIdleMoveState : IPlayerState
{
    private readonly PlayerController player;

    public PlayerIdleMoveState(PlayerController player)
    {
        this.player = player;
    }

    public void Enter() { }

    public void UpdateState() { }

    public void FixedUpdateState() 
    { 
        if (player.Motor != null)
        {
            player.Motor.Move(player.CurrentInputVector, player.MoveSpeed);
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