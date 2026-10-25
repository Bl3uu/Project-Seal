using UnityEngine;
using System.Collections;

public class KnockbackReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EntityMotor motor;
    [SerializeField] private PlayerController playerController;

    [Header("Knockback Settings")]
    [SerializeField] private bool canBeKnockedBack = true;
    [SerializeField, Range(0f, 1f)] private float knockbackResistance = 0f;

    private bool isKnockedBack;
    private Coroutine knockbackCoroutine;

    public bool IsKnockedBack => isKnockedBack;

    public void Awake()
    {
        if (rb == null) 
        {
            rb = GetComponent<Rigidbody2D>();
        }
        if (motor == null)
        {
            motor = GetComponent<EntityMotor>();
        }
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (!canBeKnockedBack || force <= 0f || knockbackResistance >= 1f)
        {
            return;
        }

        isKnockedBack = true;

        if (motor != null)
        {
            motor.IsKnockedBack = true;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            float finalForce = force * (1f - knockbackResistance);
            rb.AddForce(direction.normalized * finalForce, ForceMode2D.Impulse);
        }

        if (playerController != null)
        {
            playerController.ApplyKnockback(duration);
            return;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(ResetKnockbackRoutine(duration));

    }

    private IEnumerator ResetKnockbackRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        isKnockedBack = false;

        if (motor != null)
        {
            motor.IsKnockedBack = false;
        }
    }
}
