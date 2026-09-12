using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private float baseDamage = 25f;

    private void Awake()
    {
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }

    public void ExecuteSlash(ComboStepData stepData)
    {
        Debug.Log($"[MeleeAttack] Executing Slash - Step {stepData.StepIndex}");

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            attackOrigin.position,
            new Vector2(1.5f, 1.5f),
            0f,
            stepData.AimDirection,
            1.2f
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != transform.root.gameObject)
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    DamageData payload = new DamageData();

                    payload.Amount = baseDamage * stepData.StepIndex;
                    payload.HitDirection = stepData.AimDirection;
                    payload.Source = transform.root.gameObject;

                    damageable.TakeDamage(payload);
                }
            }
        }
    }
}
