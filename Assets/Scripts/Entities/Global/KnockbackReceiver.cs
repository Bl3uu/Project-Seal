using UnityEngine;
using System.Collections;

public class KnockbackReceiver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField, Range(0f, 1f)] private float knockbackResistance = 0f;

    private bool isKnockedBack;
    private Coroutine knockbackCoroutine;

    public bool IsKnockedBack => isKnockedBack;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private IEnumerator ResetKnockbackRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.linearVelocity = Vector2.zero;

        isKnockedBack = false;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (force <= 0f)
        {
            return;
        }

        isKnockedBack = true;
        rb.linearVelocity = Vector2.zero;

        float finalForce = force * (1f - knockbackResistance);
        rb.AddForce(direction.normalized * finalForce, ForceMode2D.Impulse);

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(ResetKnockbackRoutine(duration));

    }
}
