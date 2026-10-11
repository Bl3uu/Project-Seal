using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(EntityMotor))]
public class PlayerDash : MonoBehaviour 
{
    [Header("Dash Parameters")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("References")]
    [SerializeField] private Health healthComponent;
    [SerializeField] private PlayerAttack playerAttack;

    private EntityMotor playerMotor;
    private IAimProvider aimProvider;
    private bool isDashing;
    private bool canDash = true;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        playerMotor = GetComponent<EntityMotor>();

        if (healthComponent == null)
        {
            healthComponent = GetComponent<Health>();
        }
        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
        }

        // Grab IAimProvider interface from the PlayerAim on this object
        aimProvider = GetComponent<IAimProvider>();
    }

    // Call this via Input System event or Update key check
    public void OnDash(InputValue value)
    {
        Debug.Log("Dash input pressed!");
        if (value.isPressed && canDash && !isDashing)
        {
            
            if (playerAttack != null)
            {
                // Check if player in middle of attack (active or recovery)
                if (playerAttack.IsAttacking)
                {
                    // Block dash if in active hitframe
                    if (!playerAttack.TryCancelAttack())
                    {
                        Debug.Log("Dash Blocked");
                        return;
                    }
                }
                // If idle but holding an acitve combo step
                else if (playerAttack.CurrentComboStep > 0)
                {
                    playerAttack.NotifyActionInterrupt();
                }
            }

            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Get current aim direction (or fallback to EntityMotor direction if aim is zero)
        Vector2 dashDirection;
        if (aimProvider != null)
        {
            dashDirection = aimProvider.AimDirection;
        }
        else
        {
            dashDirection = Vector2.down;
        }

        if (dashDirection == Vector2.zero)
        {
            dashDirection = Vector2.down;
        }

        dashDirection = dashDirection.normalized;

        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.fixedDeltaTime;

            playerMotor.Move(dashDirection, dashSpeed);

            yield return new WaitForFixedUpdate();
        }

        // Stop dash velocity
        playerMotor.Move(Vector2.zero, 0f);
        isDashing = false;

        // Cooldown timer before player can dash again
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
