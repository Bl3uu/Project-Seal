using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAimController : MonoBehaviour, IAimProvider
{
    [Header("Custom Cursor Settings")]
    [SerializeField] private Texture2D cursorTexture;
    [SerializeField] private Vector2 hotspot = Vector2.zero; // (0,0) is top-left tip

    [Header("References")]
    [SerializeField] private Camera mainCamera;

    public Vector2 AimDirection { get; private set; }
    public Vector2 AimWorldPosition { get; private set; }

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Apply custom cursor on start
        if (cursorTexture == null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
    }

    private void Update()
    {
        CalculateAim();
    }

    private void CalculateAim()
    {
        if (Mouse.current == null)
        {
            return;
        }

        // Read mouse screen position
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        // Convert to world coordinates
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));

        AimWorldPosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // Calculate direction vector from player to mouse world position
        Vector2 playerPos = transform.position;

        // Avoid division by zero if mouse is directly on top of player center
        Vector2 rawDirection = AimWorldPosition - playerPos;

        if (rawDirection.sqrMagnitude > 0.001f)
        {
            AimDirection = rawDirection.normalized;
        }
    }

    // Draw debug line in scene view to visualise aim direction
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + AimDirection * 2f);
    }
}
