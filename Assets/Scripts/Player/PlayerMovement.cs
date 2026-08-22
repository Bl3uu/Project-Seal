using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float castDistance = 0.15f;
    [SerializeField] private LayerMask collisionLayer;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector2 moveDirection;

    // Public properties for PlayerAnimation script
    public Vector2 MoveDirection => moveDirection;
    public bool IsMoving => moveDirection != Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        moveDirection = ReadInputVector();
    }

    private void FixedUpdate()
    {
        ApplyMovement(moveDirection);
    }

    // --- Custom Functions ---

    /// Read WASD and Arrow keys and returns normalised direction vector.
    private Vector2 ReadInputVector()
    {
        float x = 0f;
        float y = 0f;

        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
        }
        
        // .normalise so diagonal movement is not faster
        return new Vector2(x, y);
    }

    /// Receives a direction and applies velocity to the rigidbody.
    private void ApplyMovement(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Check X axis independently
        if (direction.x !=0 && HasWallInDirection(new Vector2(direction.x, 0)))
        {
            direction.x = 0f; // Block X movement into walls
        }

        // Check Y axis independently
        if (direction.y != 0 && HasWallInDirection(new Vector2(0, direction.y)))
        { 
            direction.y = 0f; // Block Y movement into walls
        }

        rb.linearVelocity = direction.normalized * moveSpeed;
    }

    private bool HasWallInDirection(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.BoxCast(
            boxCollider.bounds.center,
            boxCollider.size,
            0f,
            direction,
            castDistance,
            collisionLayer
        );

        return hit.collider != null;
    }
}