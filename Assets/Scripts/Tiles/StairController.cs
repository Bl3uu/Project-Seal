using UnityEngine;
using static LandingTrigger;

public class StairController : MonoBehaviour
{
    public enum StairDirection { North, South, East, West }

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

        // Entering either landing pad always puts/keeps entity in Upper Layer and active stair state
        elevation.IsOnStairs = true;
        elevation.SetElevation(upperLayer, upperLayer);

        if (other.TryGetComponent<IMovementController>(out var movement))
        {
            ApplyYBias(movement);
        }
    }

    public void OnLandingTriggerExited(Collider2D other, LandingTrigger.LandingType exitedLanding)
    {
        if (!other.TryGetComponent<ElevationHandler>(out var elevation))
        {
            return;
        }

        Vector2 moveDir = Vector2.zero;

        bool hasMovable = other.TryGetComponent<IMovementController>(out var movement);

        if (hasMovable)
        {
            moveDir = movement.MoveDirection;
        }

        bool isExitingToFlatGround = IsExitingToFlatGround(exitedLanding, moveDir);

        if (isExitingToFlatGround)
        {
            // Revert stair state and bias
            elevation.IsOnStairs = false;

            if (hasMovable)
            {
                movement.StairYBias = 0f;
            }

            string targetLayer;

            if (exitedLanding == LandingTrigger.LandingType.Upper)
            {
                targetLayer = upperLayer;
            }
            else
            {
                targetLayer = lowerLayer;
            }

            elevation.SetElevation(targetLayer, targetLayer);
        }
        else
        {
            // Steps back into stair slope
            elevation.IsOnStairs = true;
            elevation.SetElevation(upperLayer, upperLayer);

            if (hasMovable)
            {
                ApplyYBias(movement);
            }
        }
    }

    private bool IsExitingToFlatGround(LandingTrigger.LandingType landing, Vector2 moveDir)
    {
        // Branch vertical vs horizontal logic cleanly
        if (stairDirection == StairDirection.North || stairDirection == StairDirection.South)
        {
            return isExitingVerticalToFlatGround(landing, moveDir);
        }
        else
        {
            return IsExitingHorizontalToFlatGround(landing, moveDir);
        }
    }

    // Vertical Stair
    private bool isExitingVerticalToFlatGround(LandingTrigger.LandingType landing, Vector2 moveDir)
    {
        if (stairDirection == StairDirection.North)
        {
            // North: Lower is at bottom (-Y), Upper landing is at top (+Y)
            if (landing == LandingTrigger.LandingType.Lower && moveDir.y < 0f)
            {
                return true;
            }
            if (landing == LandingTrigger.LandingType.Upper && moveDir.y > 0f)
            {
                return true;  
            }
        }
        else if (stairDirection == StairDirection.South)
        {
            // South: Lower landing is at top (+Y), Upper landing is at bottom (-Y)
            if (landing == LandingTrigger.LandingType.Lower && moveDir.y > 0f)
            {
                return true;
            }
            if (landing == LandingTrigger.LandingType.Upper && moveDir.y < 0f)
            {
                return true;
            }
        }
        return false;
    }

    // Horizontal Stair
    private bool IsExitingHorizontalToFlatGround(LandingTrigger.LandingType landing, Vector2 moveDir)
    {
        if (stairDirection == StairDirection.East)
        {
            // East (Descends Down-Right) Upper landing is Left (-X), Lower landing is Right (+X)
            if (landing == LandingTrigger.LandingType.Lower && moveDir.x > 0f)
            {
                return true;
            }
            if (landing == LandingTrigger.LandingType.Upper && moveDir.x < 0f)
            {
                return true;
            }
        }
        else if (stairDirection == StairDirection.West)
        {
            // West (Descend Down-Left): Upper landing is Righht (+X), Lower landing is left Left (-X)
            if (landing == LandingTrigger.LandingType.Lower && moveDir.x < 0f)
            {
                return true;
            }
            if (landing == LandingTrigger.LandingType.Upper && moveDir.x < 0f)
            {
                return true;
            }
        }
        return false;
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
