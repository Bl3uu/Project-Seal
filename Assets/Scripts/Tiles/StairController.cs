using UnityEngine;

public class StairController : MonoBehaviour
{
    public enum StairDirection { North, South, East, West}

    [Header("Stair Configuration")]
    [Tooltip("North/South = Vertical stairs (no Y-bias). \nEast = Descends Down-Right. \nWest = Descend Down-Left.")]
    [SerializeField] private StairDirection stairDirection = StairDirection.North;

    [Tooltip("How strongly the entity shifts up/down along Y while moving along X (Horizontal stairs only).")]
    [SerializeField] private float slopeRatio = 0.5f;

    [Header("Elevation Layers")]
    [Tooltip("The lower floor physics/sorting layer name")]
    [SerializeField] private string lowerLayer = "Layer 1";

    [Tooltip("The upper floor physics/sorting layer name")]
    [SerializeField] private string upperLayer = "Layer 2";

    public void OnLandingTriggerEntered(Collider2D other, LandingTrigger.LandingType landingType)
    {
        if (!other.TryGetComponent<ElevationHandler>(out var elevation))
        {
            return;
        }

        bool hasMovable = other.TryGetComponent<IMovementController>(out var movement);

        if (!elevation.IsOnStairs)
        {
            // Entering stairs from either landing
            elevation.IsOnStairs = true;
            elevation.SetElevation(upperLayer, upperLayer);

            if (hasMovable)
            {
                ApplyYBias(movement);
            }
        }
        else
        {
            // Reached the destination landing
            elevation.IsOnStairs = false;

            if (hasMovable)
            {
                movement.StairYBias = 0f;
            }

            string targetLayer;

            if (landingType == LandingTrigger.LandingType.Upper)
            {
                targetLayer = upperLayer;
            }
            else
            {
                targetLayer = lowerLayer;
            }

            elevation.SetElevation(targetLayer, targetLayer);
        }
    }

    private void ApplyYBias(IMovementController movement)
    {
        if (stairDirection == StairDirection.North || stairDirection == StairDirection.South)
        {
            return;
        }

        float directionMultiplier;
        if (stairDirection == StairDirection.East)
        {
            directionMultiplier = -1f;
        }
        else
        {
            directionMultiplier = 1f;
        }

        movement.StairYBias = slopeRatio * directionMultiplier;
    }
}
