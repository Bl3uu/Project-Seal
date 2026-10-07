using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private VFXOverlayDriver vfxDriver;

    private AttackData lastAttackData;
    private Vector2 lastAimDirection;

    private void Awake()
    {
        if (attackOrigin == null)
        {
            attackOrigin = transform;
        }
    }

    public void ExecuteSlash(AttackData attackData, Vector2 aimDirection, int comboStep)
    {
        if (attackData == null)
        {
            return;
        }

        Debug.Log("[MeleeAttack] Executing Slash");

        Vector2 normalizedAimDir = aimDirection.normalized;
        float aimAngle = Mathf.Atan2(normalizedAimDir.y, normalizedAimDir.x) * Mathf.Rad2Deg;

        DamageData payload = new DamageData();

        payload.Amount = attackData.baseDamage;
        payload.HitDirection = normalizedAimDir;
        payload.KnockbackForce = attackData.knockbackForce; ;
        payload.KnockbackDuration = attackData.knockbackDuration;
        payload.Source = transform.root.gameObject;

        if (vfxDriver != null)
        {
            MeleeHitbox hitbox = vfxDriver.TriggerSlashVFX(comboStep, aimAngle);
            if (hitbox != null)
            {
                hitbox.Initialize(payload, attackData.activeHitDuration);
            }
            else
            {
                Debug.LogWarning($"[MeleeAttack] No MeleeHitbox component found on VFX prefab for combo step {comboStep}");
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
