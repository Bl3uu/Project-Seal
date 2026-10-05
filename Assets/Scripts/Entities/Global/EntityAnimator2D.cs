using UnityEngine;

public class EntityAnimator2d : MonoBehaviour, IAnimationController
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [Header("Settings")]
    [Tooltip("True for Player, False for simple walking enemies")]
    [SerializeField] private bool faceAimDirection = true;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    private static readonly int IsDeadParam = Animator.StringToHash("IsDead");

    public void UpdateLocomotion(Vector2 movementInput, Vector2 lookDirection)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedParam, movementInput.magnitude);
        }

        Vector2 targetDir;

        if (faceAimDirection && lookDirection != Vector2.zero)
        {
            targetDir = lookDirection;
        }
        else
        {
            targetDir = movementInput;
        }

        if (targetDir.x != 0 && spriteRenderer != null)
        {
            // Flip sprite left if aiming or moving left, right if aiming or moving right
            spriteRenderer.flipX = targetDir.x < 0;
        }
    }

    public void PlayerAttack(int comboStep, float aimAngle)
    {
        // Body level attack trigger
    }

    public void PlayHurt()
    {
        if (animator != null)
        {
            animator.SetTrigger(HurtTrigger);
        }
    }

    public void PlayDeath()
    {
        if (animator != null)
        {
            animator.SetBool(IsDeadParam, true);
        }
    }
}
