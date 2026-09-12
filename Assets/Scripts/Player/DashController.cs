using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class DashController : MonoBehaviour 
{
    [Header("Dash Parameters")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private PlayerAttack playerAttack;

    private IAimProvider aimProvider;
    private bool isDashing;
    private bool canDash = true;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (healthComponent == null)
        {
            healthComponent = GetComponent<HealthComponent>();
        }
        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
        }

        // Grab IAimProvider interface from the MouseAimController on this object
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
                    playerAttack.ResetCombo();
                }
            }

            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Get current aim direction (or fallback to movement direction if aim is zero)
        Vector2 dashDirection;
        if (aimProvider != null)
        {
            dashDirection = aimProvider.AimDirection;
        }
        else
        {
            dashDirection = Vector2.down;
        }

        float timer = 0f;

        while (timer < dashDuration)
        {
            timer += Time.fixedDeltaTime;

            // Move along the aim vector using Rigidbody physics
            rb.linearVelocity = dashDirection * dashSpeed;

            yield return new WaitForFixedUpdate();
        }

        // Stop dash velocity
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        // Cooldown timer before player can dash again
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
