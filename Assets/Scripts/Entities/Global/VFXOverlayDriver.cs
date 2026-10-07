using UnityEngine;

public class VFXOverlayDriver : MonoBehaviour
{
    [Header("Pivot & Anchors")]
    [Tooltip("Rotates around character centre toward cursor")]
    [SerializeField] private Transform weaponPivot;

    [Header("VFX Prefabs / Pool")]
    [Tooltip("Assigned Per comboStep index")]
    [SerializeField] private GameObject[] meleeSlashPrefabs;
    [SerializeField] private ParticleSystem muzzleFlashVFX;

    public MeleeHitbox TriggerSlashVFX(int comboStep, float aimAngle)
    {
        if (weaponPivot == null)
        {
            return null;
        }

        weaponPivot.rotation = Quaternion.Euler(0f, 0f, aimAngle);

        int index = Mathf.Clamp(comboStep - 1, 0, meleeSlashPrefabs.Length - 1);
        if (meleeSlashPrefabs.Length > 0 && meleeSlashPrefabs[index] != null)
        {
            GameObject vfxInstance = Instantiate(meleeSlashPrefabs[index], weaponPivot.position, weaponPivot.rotation);
            return vfxInstance.GetComponent<MeleeHitbox>();
        }

        return null;
    }

    public void TriggerMuzzleFlash(float aimAngle)
    {
        if (weaponPivot == null)
        {
            return;
        }

        weaponPivot.rotation = Quaternion.Euler(0f, 0f, aimAngle);
        if (muzzleFlashVFX != null)
        {
            muzzleFlashVFX.Play();
        }
    }
}
