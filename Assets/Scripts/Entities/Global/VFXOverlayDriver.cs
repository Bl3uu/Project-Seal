using UnityEngine;

public class VFXOverlayDriver : MonoBehaviour
{
    [Header("Pivot & Anchors")]
    [Tooltip("Rotates around character centre toward cursor")]
    [SerializeField] private Transform weaponPivot;

    public GameObject PlayVFX(GameObject vfxPrefab, float aimAngle, float attackDistance)
    {
        if (weaponPivot == null || vfxPrefab == null)
        {
            return null;
        }

        weaponPivot.rotation = Quaternion.Euler(0f, 0f, aimAngle);

        GameObject vfxInstance = Instantiate(vfxPrefab, weaponPivot.position, weaponPivot.rotation, weaponPivot);

        vfxInstance.transform.localPosition = new Vector3(attackDistance, 0f, 0f);

        return vfxInstance;
    }

    public MeleeHitbox SpawnAttackVFX(GameObject vfxPrefab, float aimAngle, float attackDistance)
    {
        if (weaponPivot == null || vfxPrefab == null)
        {
            return null;
        }

        weaponPivot.rotation = Quaternion.Euler(0f, 0f, aimAngle);

        GameObject vfxInstance = Instantiate(vfxPrefab, weaponPivot.position, weaponPivot.rotation, weaponPivot);

        vfxInstance.transform.localPosition = new Vector3(attackDistance, 0f, 0f);

        return vfxInstance.GetComponent<MeleeHitbox>();
    }
}
