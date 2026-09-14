using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private Transform attackOrigin;

    private AttackData lastAttackData;
    private Vector2 lastAimDirection;

    private void Awake()
    {
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }

    public void ExecuteSlash(AttackData attackData, Vector2 aimDirection)
    {
        Debug.Log("[MeleeAttack] Executing Slash");

        lastAttackData = attackData;
        lastAimDirection = aimDirection.normalized;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            attackOrigin.position,
            attackData.hitboxSize,
            0f,
            lastAimDirection,
            attackData.attackDistance
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != transform.root.gameObject)
            {
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    DamageData payload = new DamageData();

                    payload.Amount = attackData.baseDamage;
                    payload.HitDirection = lastAimDirection;
                    payload.KnockbackForce = attackData.knockbackForce; ;
                    payload.KnockbackDuration = attackData.knockbackDuration;
                    payload.Source = transform.root.gameObject;

                    damageable.TakeDamage(payload);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (lastAttackData == null || attackOrigin == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Vector3 boxCenter = attackOrigin.position + (Vector3)(lastAimDirection * lastAttackData.attackDistance);
        Gizmos.DrawWireCube(boxCenter, lastAttackData.hitboxSize);
    }
}
