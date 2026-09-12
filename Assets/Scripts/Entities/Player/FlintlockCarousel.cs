using UnityEngine;

public class FlintlockCarousel : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private int maxBarrels = 4;

    private int currentLoadedBarrels;

    private void Awake()
    {
        currentLoadedBarrels = maxBarrels;
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    public void FireComboShot(ComboStepData stepData)
    {
        if (currentLoadedBarrels <= 0)
        {
            Debug.Log("[FlintlockCarousel Click! Out of loaded barrels.");
            return;
        }

        currentLoadedBarrels--;
        Debug.Log($"[FlintlockCarousel] Fire Combo Shot - Step {stepData.StepIndex}");

        SpawnProjectile(stepData.AimDirection);
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

        SpawnProjectile(aimDirection);
    }

    private void SpawnProjectile(Vector2 direction)
    {
        if (bulletPrefab != null)
        {
            // Instatiate projectile prefab with velocity/direction
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            // Bullet setup logic next
        }
    }

    public void ReloadCarousel()
    {
        // Change later
        currentLoadedBarrels = maxBarrels;
    }
}
