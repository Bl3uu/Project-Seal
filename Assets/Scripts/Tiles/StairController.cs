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
        if (!other.TryGetComponent<IElevationHandler>(out var elevation))
        {
            return;
        }

        other.TryGetComponent<IMovementController>(out var movement);
        EnterStairState(elevation, movement);
    }

    public void OnLandingTriggerExited(Collider2D other, LandingTrigger.LandingType exitedLanding)
    {
        if (!other.TryGetComponent<IElevationHandler>(out var elevation))
        {
            return;
        }

        other.TryGetComponent<IMovementController>(out var movement);

        Vector2 moveDir;

        if (movement != null)
        {
            moveDir = movement.MoveDirection;
        }
        else
        {
            moveDir = Vector2.zero;
        }
        
        if (IsExitingToFlatGround(exitedLanding, moveDir))
        {
            string targetLayer;

            if (exitedLanding == LandingTrigger.LandingType.Upper)
            {
                targetLayer = upperLayer;
            }
            else
            {
                targetLayer = lowerLayer;
            }

            ExitStairState(elevation, movement, targetLayer);
        }
        else
        {
            EnterStairState(elevation, movement);
        }
    }

    private void EnterStairState(IElevationHandler elevation, IMovementController movement)
    {
        elevation.IsOnStairs = true;
        elevation.SetElevation(upperLayer, upperLayer);

        if (movement != null)
        {
            ApplyYBias(movement);
        }
    }

    private void ExitStairState(IElevationHandler elevation, IMovementController movement, string targetLayer)
    {
        elevation.IsOnStairs = false;
        elevation.SetElevation(targetLayer, targetLayer);

        if (movement != null)
        {
            movement.StairYBias = 0f;
        }
    }

    private bool IsExitingToFlatGround(LandingTrigger.LandingType landing, Vector2 moveDir)
    {
        if (moveDir == Vector2.zero)
        {
            return false;
        }

        switch (stairDirection)
        {
            case StairDirection.North:
                // Lower landing is at bottom (-Y), Upper landing is at top (+Y)
                if (landing == LandingTrigger.LandingType.Lower && moveDir.y < 0f)
                {
                    return true;
                }
                if (landing == LandingTrigger.LandingType.Upper && moveDir.y > 0f)
                {
                    return true;
                }
                break;

            case StairDirection.South:
                // Lower landing is at top (+Y), Upper landing is at bottom (-Y)
                if (landing == LandingTrigger.LandingType.Lower && moveDir.y > 0f)
                {
                    return true;
                }
                if (landing == LandingTrigger.LandingType.Upper && moveDir.y < 0f)
                {
                    return true;
                }
                break;

            case StairDirection.East:
                //Upper landing is Left (-X), Lower landing is Right (+X)
                if (landing == LandingTrigger.LandingType.Lower && moveDir.x > 0f)
                {
                    return true;
                }
                if (landing == LandingTrigger.LandingType.Upper && moveDir.x < 0f)
                {
                    return true;
                }
                break;

            case StairDirection.West:
                // Upper landing is Right (+X), Lower landing is Left (-X)
                if (landing == LandingTrigger.LandingType.Lower && moveDir.x < 0f)
                {
                    return true;
                }
                if (landing == LandingTrigger.LandingType.Upper && moveDir.x > 0f)
                {
                    return true;
                }
                break;

            default:
                return false;
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
