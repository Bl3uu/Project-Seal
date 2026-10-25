using UnityEngine;

public class PlayerKnockbackState : IPlayerState
{
    private readonly PlayerController player;
    private float knockbackTimer;
    private float currentDuration;

    public PlayerKnockbackState(PlayerController player)
    {
        this.player = player;
    }

    public void SetDuration(float duration)
    {
        currentDuration = duration;
    }

    public void Enter()
    {
        knockbackTimer = 0f;
    }

    public void UpdateState()
    {
        knockbackTimer += Time.deltaTime;

        if (knockbackTimer >= currentDuration)
        {
            player.ChangeState(player.IdleMoveState);
        }
    }

    public void FixedUpdateState()
    {

    }

    public void Exit()
    {
        if (player.Motor != null)
        {
            player.Motor.Move(Vector2.zero, 0f);
        }
    }
}
