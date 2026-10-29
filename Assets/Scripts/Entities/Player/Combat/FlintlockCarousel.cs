using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(EntityMotor))]
public class FlintlockCarousel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RangeAttack rangeAttack;

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
        if (rangeAttack == null)
        {
            rangeAttack = GetComponent<RangeAttack>();
        }
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

        rangeAttack.Fire(aimDirection, attackData);
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

        rangeAttack.Fire(aimDirection, freeFireData);
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
}
