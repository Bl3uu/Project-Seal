using UnityEngine;

public class StairsElevationTrigger : MonoBehaviour
{
    public enum Direction { North, South, West, East }

    [Header("Stair Direction")]
    [Tooltip("Which direction points UP towards the higher floor")]
    public Direction direction = Direction.North;

    [Header("Upper Floor Settings")]
    public string layerUpper = "Layer 2";
    public string sortingLayerUpper = "Layer 2";

    [Header("Lower Floor Settings")]
    public string layerLower = "Layer 1";
    public string sortingLayerLower = "Layer 1";

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndSetElevation(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        
    }

    private void CheckAndSetElevation(Collider2D other)
    {
        if (other.TryGetComponent<PlayerMovement>(out PlayerMovement player))
        {
            Vector3 playerPos = other.transform.position;
            Vector3 stairPos = transform.position;

            bool isUpperSide = false;

            // Check if player is on the 'Upper' side based on stair direction
            switch (direction)
            {
                case Direction.North:
                    isUpperSide = playerPos.y > stairPos.y;
                    break;
                case Direction.South:
                    isUpperSide = playerPos.y < stairPos.y;
                    break;
                case Direction.West:
                    isUpperSide = playerPos.x < stairPos.x;
                    break;
                case Direction.East:
                    isUpperSide = playerPos.x > stairPos.x;
                    break;
            }

            if (isUpperSide)
            {
                player.SetElevation(layerUpper, sortingLayerUpper);
            }
            else
            {
                player.SetElevation(layerLower, sortingLayerLower);
            }
        }
    }
}
