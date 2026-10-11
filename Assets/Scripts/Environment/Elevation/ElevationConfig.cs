using UnityEngine;

[CreateAssetMenu(fileName = "ElevationConfig", menuName ="Elevation/Elevation Config")]
public class ElevationConfig : ScriptableObject
{
    [Header("Global ObstacleMask by Height Index")]
    [Tooltip("Mask indexes for 1st, 2nd and nth floors")]
    [SerializeField] private LayerMask[] obstacleMaskByHeight;

    public LayerMask GetMaskForHeight(int heightLevel)
    {
        if (obstacleMaskByHeight == null || obstacleMaskByHeight.Length == 0)
        {
            return ~0;
        }

        int safeIndex = Mathf.Clamp(heightLevel, 0, obstacleMaskByHeight.Length - 1);
        return obstacleMaskByHeight[safeIndex];
    }
}
