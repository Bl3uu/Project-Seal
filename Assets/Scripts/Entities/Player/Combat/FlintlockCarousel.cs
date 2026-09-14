using UnityEngine;

public class FlintlockCarousel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Carousel Capacity")]
    [SerializeField] private int maxBarrels = 4;

    private int currentLoadedBarrels;

    public int CurrentLoadedBarrels => currentLoadedBarrels;
    public int MaxBarrels => maxBarrels;

    private void Awake()
    {
        currentLoadedBarrels = maxBarrels;
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    public void FireComboShot(AttackData attackData, Vector2 aimDirection)
    {
        if (attackData == null)
        {
            return;
        }

        if (currentLoadedBarrels <= 0)
        {
            Debug.Log("[FlintlockCarousel Click! Out of loaded barrels.");
            return;
        }

        currentLoadedBarrels--;
        Debug.Log($"[FlintlockCarousel] Fire Combo Shot");

        SpawnProjectile(aimDirection, attackData);
    }

    public void FireFreeShot(Vector2 aimDirection, Vector2 aimWorldPosition)
    {
        if (currentLoadedBarrels <= 0)
        {
            Debug.Log("[FlintlockCarousel Click! Out of loaded barrels.");
            return;
        }

        currentLoadedBarrels--;
        Debug.Log($"[FlintlockCarousel] Free-Fire Shot Executed! | Barrels remaining: {currentLoadedBarrels}");

        SpawnProjectile(aimDirection, null);
    }

    private void SpawnProjectile(Vector2 direction, AttackData attackData)
    {
        if (bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            
            if (bulletObj.TryGetComponent<Bullet>(out var bullet))
            {
                float damage;
                float kbForce;
                float kbDuration;

                if (attackData != null)
                {
                    damage = attackData.baseDamage;
                    kbForce = attackData.knockbackForce;
                    kbDuration = attackData.knockbackDuration;
                }
                else
                {
                    damage = 15f;
                    kbForce = 3f;
                    kbDuration = 0.15f;
                }

                bullet.Initialize(direction, damage, kbForce, kbDuration, transform.root.gameObject);
            }
        }
    }

    public void ReloadCarousel()
    {
        // Change later
        currentLoadedBarrels = maxBarrels;
    }
}
