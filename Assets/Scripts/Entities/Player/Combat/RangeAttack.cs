using UnityEngine;

public class RangeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private VFXOverlayDriver vfxDriver;

    private EntityMotor motor;
    private EntityAudioDriver audioDriver;
    private DataDrivenVFXPlayer vfxPlayer;

    private void Awake()
    {
        motor = GetComponent<EntityMotor>();
        audioDriver = GetComponentInParent<EntityAudioDriver>();

        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    public void Fire(Vector2 aimDirection, AttackData attackData)
    {
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        if (attackData != null && attackData.vfxPrefab != null && vfxDriver != null)
        {
            vfxPlayer = vfxDriver.SpawnAttackVFX(attackData.vfxPrefab, aimAngle, attackData.attackDistance);
            if (vfxPlayer != null)
            {
                vfxPlayer.InitializeAndPlay(attackData);
            }
        }

        if (audioDriver != null && attackData != null && attackData.attackSFX != null)
        {
            audioDriver.PlaySound(attackData.attackSFX);
        }

        if (motor != null && attackData != null && attackData.lungeForce > 0f)
        {
            Vector2 recoilDirection = -aimDirection.normalized;
            Debug.Log($"[FlintlockCarousel] Applying Recoil Vector: {-aimDirection.normalized}");
            motor.Move(recoilDirection, attackData.lungeForce);
        }

        if (bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                float damage;
                float poiseDamage;
                float kbForce;
                float kbDuration;

                if (attackData != null)
                {
                    damage = attackData.baseDamage;
                    poiseDamage = attackData.poiseDamage;
                    kbForce = attackData.knockbackForce;
                    kbDuration = attackData.knockbackDuration;
                }
                else
                {
                    damage = 15f;
                    poiseDamage = 25f;
                    kbForce = 3f;
                    kbDuration = 0.15f;
                }

                bullet.Initialize(aimDirection, damage, poiseDamage, kbForce, kbDuration, transform.root.gameObject);
            }
        }
    }
}
