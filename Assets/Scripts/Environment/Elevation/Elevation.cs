using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Elevation : MonoBehaviour, IElevatable
{
    private SpriteRenderer spriteRenderer;

    [Header("Elevation State")]
    [SerializeField] private int currentHeight = 0;

    public int CurrentHeight => currentHeight;
    public bool IsOnStairs { get; set; } = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetElevation(int heightLevel, string sortingLayerName)
    {
        currentHeight = heightLevel;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = sortingLayerName;
        }

        // If entity has PlayerMovement, update its custom BoxCast mask
        if (TryGetComponent<EntityMotor>(out var motor))
        {
            motor.UpdateHeightLevel(currentHeight);
        }
    }
}
