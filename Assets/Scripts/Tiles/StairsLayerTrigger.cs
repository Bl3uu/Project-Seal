using UnityEngine;

public class StairsElevationTrigger : MonoBehaviour
{
    public enum Direction { North, South, West, East }

    [Header("Floor Settings")]
    [Tooltip("The Physics Layer to apply to the player (e.g., Layer 1 or Layer 2)")]
    public string targetLayer = "Layer 1";
    [Tooltip("The Sorting Layer to apply to the player (e.g., Layer 1 or Layer 2)")]
    public string targetSortingLayer = "Layer 1";

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[StairTrigger] Collided with: {other.name} on Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        if (other.TryGetComponent<ElevationHandler>(out ElevationHandler entity))
        {
            entity.SetEvelation(targetLayer, targetSortingLayer);
        }
        else
        {
            Debug.LogWarning($"[StairTrigger] {other.name} entered trigger, but lacks ElevationHandler component!");
        }
    }
}
