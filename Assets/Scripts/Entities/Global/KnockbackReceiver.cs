using UnityEngine;
using System.Collections;

public class KnockbackReceiver : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private bool canBeKnockedBack = true;
    [SerializeField, Range(0f, 1f)] private float knockbackResistance = 0f;

    private IKnockbackable knockbackableTarget;

    public void Awake()
    {
        knockbackableTarget = GetComponent<IKnockbackable>();
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        if (!canBeKnockedBack || force <= 0f || knockbackResistance >= 1f)
        {
            Debug.Log("[Knockback] Blocked: CanBeKnockedBack is false, force is <= 0, or resistance >= 1.");
            return;
        }

        float finalForce = force * (1f - knockbackResistance);

        if (knockbackableTarget != null)
        {
            knockbackableTarget.ApplyKnockback(direction, finalForce, duration);
        }
    }
}
