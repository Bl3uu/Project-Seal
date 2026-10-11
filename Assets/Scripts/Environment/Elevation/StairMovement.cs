using System;
using UnityEngine;
using static LandingTrigger;

public class StairMovement : MonoBehaviour
{
    public enum StairDirection { North, South, East, West }

    [Header("Stair Configuration")]
    [Tooltip("North/South = Vertical stairs (no Y-bias). \nEast = Descends Down-Right. \nWest = Descend Down-Left.")]
    [SerializeField] private StairDirection stairDirection = StairDirection.North;

    [Tooltip("How strongly the entity shifts up/down along Y while moving along X (Horizontal stairs only).")]
    [SerializeField] private float slopeRatio = 0.5f;

    [Header("Elevation Layers")]
    [SerializeField] private int lowerHeight = 0;
    [SerializeField] private int upperHeight = 1;

    [Header("Sorting Layers")]
    [SerializeField] private string lowerSortingLayer = "Layer 1";
    [SerializeField] private string upperSortingLayer = "Layer 2";

    public void OnLandingTriggerEntered(Collider2D other, LandingTrigger.LandingType landingType)
    {
        if (!other.TryGetComponent<IElevatable>(out var elevation))
        {
            return;
        }

        other.TryGetComponent<IMoveable>(out var movement);
        EnterStairState(elevation, movement);
    }

    public void OnLandingTriggerExited(Collider2D other, LandingTrigger.LandingType exitedLanding)
    {
        if (!other.TryGetComponent<IElevatable>(out var elevation))
        {
            return;
        }

        other.TryGetComponent<IMoveable>(out var movement);

        Vector2 moveDir = Vector2.zero;

        if (movement != null && movement.MoveDirection != Vector2.zero)
        {
            moveDir = movement.MoveDirection;
        }
        else if (other.TryGetComponent<Rigidbody2D>(out var rb))
        {
            moveDir = rb.linearVelocity.normalized;
        }
        
        if (IsExitingToFlatGround(exitedLanding, moveDir))
        {
            int targetHeight;
            string targetSortingLayer;

            if (exitedLanding == LandingTrigger.LandingType.Upper)
            {
                targetHeight = upperHeight;
                targetSortingLayer = upperSortingLayer;
            }
            else
            {
                targetHeight = lowerHeight;
                targetSortingLayer = lowerSortingLayer;
            }

            ExitStairState(elevation, movement, targetHeight, targetSortingLayer);
        }
        else
        {
            EnterStairState(elevation, movement);
        }
    }

    private void EnterStairState(IElevatable elevation, IMoveable movement)
    {
        elevation.IsOnStairs = true;
        elevation.SetElevation(upperHeight, upperSortingLayer);

        if (movement != null)
        {
            ApplyYBias(movement);
        }
    }

    private void ExitStairState(IElevatable elevation, IMoveable movement, int targetHeight, string targetSortingLayer)
    {
        elevation.IsOnStairs = false;
        elevation.SetElevation(targetHeight, targetSortingLayer);

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

    private void ApplyYBias(IMoveable movement)
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
