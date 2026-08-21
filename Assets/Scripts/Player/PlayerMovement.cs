using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Sprite References")]
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite sideSprite;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        moveDirection = ReadInputVector();
        if (moveDirection != Vector2.zero)
        {
            UpdateSpriteOrientation(moveDirection);
        }
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
        return new Vector2(x, y).normalized;
    }

    /// Receives a direction and applies velocity to the rigidbody.
    private void ApplyMovement(Vector2 direction)
    {
        rb.linearVelocity = direction * moveSpeed;
    }

    /// 3/4 perpective flipping
    private void UpdateSpriteOrientation(Vector2 input)
    {
        // Horizontal Movement Priority
        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
        {
            spriteRenderer.sprite = sideSprite;
            if (input.x > 0f)
            {
                spriteRenderer.flipX = false; // Face Right
            }
            else if (input.x < 0f)
            {
                spriteRenderer.flipX = true; // Face Left (Mirrored)
            }
        }
        // Vertical Movement Priority
        else
        {
            spriteRenderer.flipX = false; // Reset flip for cardinal up/down

            if (input.y > 0f)
            {
                spriteRenderer.sprite = upSprite;
            }
            else if (input.y < 0f)
            {
                spriteRenderer.sprite = downSprite;
            }
        }
    }
}