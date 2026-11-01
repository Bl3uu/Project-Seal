using UnityEngine;

public class PlayerKnockbackState : IPlayerState
{
    private readonly PlayerController player;
    private Rigidbody2D rb;

    private Vector2 slideDirection;
    private float initialForce;
    private float currentDuration;
    private float knockbackTimer;

    public PlayerKnockbackState(PlayerController player)
    {
        this.player = player;
        if (player != null) {
            rb = player.GetComponent<Rigidbody2D>();
        }
    }

    public void SetupKnockback(Vector2 direction, float force, float duration)
    {
        slideDirection = direction.normalized;
        initialForce = force;
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
        if (rb != null && currentDuration > 0f)
        {
            float progress = Mathf.Clamp01(knockbackTimer / currentDuration);
            float currentSpeed = Mathf.Lerp(initialForce, 0f, progress);
            rb.linearVelocity = slideDirection * currentSpeed;
        }
    }

    public void Exit()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
