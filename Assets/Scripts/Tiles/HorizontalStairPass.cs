using UnityEngine;

public class HorizontalStairPass : MonoBehaviour 
{
    [Header("Slope Configuration")]
    [Tooltip("How strongly the player shifts up/down along Y while moving on X")]
    [SerializeField] private float slopeRatio = 0.5f;

    [Tooltip("Check True if moving RIGHT goes UP. Uncheck if moving RIGHT goes DOWN")]
    [SerializeField] private bool rightIsUp = true;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null)
        {
            float directionMultiplier;
            if (rightIsUp)
            {
                directionMultiplier = 1f;
            }
            else
            {
                directionMultiplier = -1f;
            }

            player.StairYBias = slopeRatio * directionMultiplier;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null)
        {
            player.StairYBias = 0f;
        }
    }
}
