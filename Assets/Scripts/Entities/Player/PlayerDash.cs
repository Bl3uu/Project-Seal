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
    [SerializeField] private float inputBufferWindow = 0.15f;

    [Header("References")]
    [SerializeField] private Health healthComponent;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private PlayerMovement playerMovement;

    private EntityMotor playerMotor;
    private IAimProvider aimProvider;
    private PlayerInputBuffer<PlayerActionType> inputBuffer;
    private bool isDashing;
    private bool canDash = true;

    public bool IsDashing => isDashing;

    private void Awake()
    {
        playerMotor = GetComponent<EntityMotor>();
        aimProvider = GetComponent<IAimProvider>();
        inputBuffer = new PlayerInputBuffer<PlayerActionType>(inputBufferWindow);

        if (healthComponent == null)
        {
            healthComponent = GetComponent<Health>();
        }
        if (playerAttack == null)
        {
            playerAttack = GetComponent<PlayerAttack>();
        }
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }
    }

    private void Update()
    {
        inputBuffer.Tick(Time.deltaTime);
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed)
        {
            return;
        }

        Debug.Log("Dash input pressed!");
        if (canDash && !isDashing)
        {
            if (playerAttack != null && playerAttack.IsAttacking)
            {
                if (playerAttack.TryCancelAttack())
                {
                    StartCoroutine(PerformDash());
                    return;
                }

                inputBuffer.Buffer(PlayerActionType.Dash);
                return;
            }

            StartCoroutine(PerformDash());
        }
        else
        {
            inputBuffer.Buffer(PlayerActionType.Dash);
        }
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;

        // Get current aim direction via entity motor movement direction
        Vector2 dashDirection = Vector2.zero;

        if (playerMovement != null)
        {
            dashDirection = playerMovement.RawInputVector;
        }

        // Fall back to aim direction (mouse cursor)
        if (dashDirection == Vector2.zero)
        {
            if (aimProvider != null)
            {
                dashDirection = aimProvider.AimDirection;
            }
            if (dashDirection == Vector2.zero)
            {
                dashDirection = Vector2.down;
            }
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

        if (inputBuffer.HasBufferedInput)
        {
            if (inputBuffer.Command == PlayerActionType.Dash)
            {
                inputBuffer.Consume();
                StartCoroutine(PerformDash());
            }
            
        }
    }
}
