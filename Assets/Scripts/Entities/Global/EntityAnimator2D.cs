using UnityEngine;

public class EntityAnimator2D : MonoBehaviour, IAnimationController
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private PlayerMovement playerMovement;
    private PlayerAim playerAim;
    private PlayerAttack playerAttack;

    [Header("Settings")]
    [Tooltip("Dynamicamically toggle: ")]
    [SerializeField] private bool faceAimDirection = false;
    [Tooltip("Uncheck this for asymmetrical character designs so sprite graphics arent mirrored horizontally.")]
    [SerializeField] private bool useSpriteFlipping = false;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int FacingXParam = Animator.StringToHash("FacingX");
    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private static readonly int HurtTrigger = Animator.StringToHash("Hurt");
    private static readonly int IsDeadParam = Animator.StringToHash("IsDead");

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerAim = GetComponent<PlayerAim>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void Update()
    {
        if (playerMovement != null)
        {
            bool isAttacking = playerAttack != null && playerAttack.IsAttacking;
            faceAimDirection = isAttacking;

            Vector2 moveDir = playerMovement.MoveDirection;
            Vector2 aimDir;

            if (playerAim != null)
            {
                aimDir = playerAim.AimDirection;
            }
            else
            {
                aimDir = Vector2.zero;
            }
            UpdateLocomotion(moveDir, aimDir);
        }
    }

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

        if (targetDir.x != 0)
        {
            if (useSpriteFlipping && spriteRenderer != null)
            {
                spriteRenderer.flipX = targetDir.x < 0;
            }
            else if (animator != null)
            {
                animator.SetFloat(FacingXParam, Mathf.Sign(targetDir.x));
            }
        }
    }

    public void PlayerAttack(int comboStep, float aimAngle)
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackTrigger);
        }
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
