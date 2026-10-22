using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(EntityMotor))]
public class FlintlockCarousel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private VFXOverlayDriver vfxDriver;
    private DataDrivenVFXPlayer vfxPlayer;

    private EntityMotor playerMotor;
    private EntityAudioDriver audioDriver;

    [Header("Carousel Capacity")]
    [SerializeField] private int maxBarrels = 4;
    [SerializeField] private float reloadDuration = 1.2f;

    private int currentLoadedBarrels;
    private bool isReloading;

    public int CurrentLoadedBarrels => currentLoadedBarrels;
    public int MaxBarrels => maxBarrels;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        currentLoadedBarrels = maxBarrels;
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        playerMotor = GetComponent<EntityMotor>();
        audioDriver = GetComponentInParent<EntityAudioDriver>();
    }

    public bool FireComboShot(AttackData attackData, Vector2 aimDirection)
    {
        if (currentLoadedBarrels <= 0)
        {
            Debug.Log("[FlintlockCarousel Click! Out of loaded barrels.");
        }

        if (attackData == null || isReloading || currentLoadedBarrels <= 0)
        {
            return false;
        }

        currentLoadedBarrels--;
        Debug.Log($"[FlintlockCarousel] Fire Combo Shot");

        SpawnProjectile(aimDirection, attackData);
        return true;
    }

    public void FireFreeShot(Vector2 aimDirection, AttackData freeFireData = null)
    {
        if (currentLoadedBarrels <= 0 || isReloading)
        {
            Debug.Log("[FlintlockCarousel Click! Out of loaded barrels.");
            return;
        }

        currentLoadedBarrels--;
        Debug.Log($"[FlintlockCarousel] Free-Fire Shot Executed! | Barrels remaining: {currentLoadedBarrels}");

        SpawnProjectile(aimDirection, freeFireData);
    }

    public void StartReload()
    {
        if (isReloading || currentLoadedBarrels == maxBarrels)
        {
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        Debug.Log("[FlintlockCarousel] Reloading");

        yield return new WaitForSeconds(reloadDuration);

        currentLoadedBarrels = maxBarrels;
        isReloading = false;
        Debug.Log("[FlintlockCarousel] Reload complete");
    }

    private void SpawnProjectile(Vector2 aimDirection, AttackData attackData)
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

        if (playerMotor != null && attackData != null && attackData.lungeForce > 0f)
        {
            Vector2 recoilDirection = -aimDirection.normalized;
            Debug.Log($"[FlintlockCarousel] Applying Recoil Vector: {-aimDirection.normalized}");
            playerMotor.Move(recoilDirection, attackData.lungeForce);
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
