using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private VFXOverlayDriver vfxDriver;

    private DataDrivenVFXPlayer vfxPlayer;

    public void PrepareAttackVFX(AttackData attackData, Vector2 aimDirection)
    {
        if (attackData == null || vfxDriver == null || attackData.vfxPrefab == null)
        {
            return;
        }

        Debug.Log("[MeleeAttack] Executing Slash");

        Vector2 normalizedAimDir = aimDirection.normalized;
        float aimAngle = Mathf.Atan2(normalizedAimDir.y, normalizedAimDir.x) * Mathf.Rad2Deg;

        DamageData payload = new DamageData();

        payload.Amount = attackData.baseDamage;
        payload.PoiseDamage = attackData.poiseDamage;
        payload.HitDirection = normalizedAimDir;
        payload.KnockbackForce = attackData.knockbackForce; ;
        payload.KnockbackDuration = attackData.knockbackDuration;
        payload.Source = transform.root.gameObject;

        vfxPlayer = vfxDriver.SpawnAttackVFX(attackData.vfxPrefab, aimAngle, attackData.attackDistance);
        if (vfxPlayer != null)
        {
            vfxPlayer.InitializeAndPlay(attackData, payload);
        }
    }
}
