using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ElevationHandler : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetEvelation(string physicsLayerName, string sortingLayerName)
    {
        //Update Physics Layer
        int layerIndex = LayerMask.NameToLayer(physicsLayerName);
        if (layerIndex != -1)
        {
            gameObject.layer = layerIndex;
        }

        // Update visual sorting Layer
        spriteRenderer.sortingLayerName = sortingLayerName;

        // If entity has PlayerMovement, update its custom BoxCast mask
        if (TryGetComponent<PlayerMovement>(out var playerMovement))
        {
            playerMovement.UpdateCollisionLayer(physicsLayerName);
        }
    }
}
