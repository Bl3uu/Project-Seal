using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour, IMovementController
{
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    private LayerMask currentCollisionLayer;
    private Rigidbody2D rb;
    private DashController dashController;
    private Vector2 moveDirection;

    public float StairYBias { get; set; } = 0f;

    public Vector2 MoveDirection => moveDirection;
    public bool IsMoving => moveDirection != Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        dashController = GetComponent<DashController>();
        UpdateCollisionLayer(LayerMask.LayerToName(gameObject.layer));
    }

    private void Update()
    {
        moveDirection = ReadInputVector();
    }

    private void FixedUpdate()
    {
        if (dashController != null && dashController.IsDashing)
        {
            return;
        }

        ApplyMovement(moveDirection);
    }

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

        return new Vector2(x, y).normalized;
    }

    private void ApplyMovement(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Apply stair bias
        if (Mathf.Abs(direction.x) > 0.01f && StairYBias != 0f)
        {
            direction.y += Mathf.Sign(direction.x) * StairYBias;
        }

        // Check if moving horizontally into a wall
        if (direction.x != 0f && IsAxisBlocked(new Vector2(direction.x, 0f)))
        {
            direction.x = 0f; // Clear X so Y gets full 1.0 speed
        }

        if (direction.y != 0f && IsAxisBlocked(new Vector2(direction.y, 0f)))
        {
            direction.y = 0f; // Clear Y so X gets full 1.0 speed
        }

        rb.linearVelocity = direction.normalized * moveSpeed;
    }

    private bool IsAxisBlocked(Vector2 axisDirection)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(currentCollisionLayer);
        filter.useLayerMask = true;
        filter.useTriggers = false; // Ignore trigger colliders such as stairs, doors etc

        RaycastHit2D[] results = new RaycastHit2D[1];
        return rb.Cast(axisDirection, filter, results, 0.05f) > 0;
    }

    public void UpdateCollisionLayer(string physicsLayerName)
    {
        gameObject.layer = LayerMask.NameToLayer(physicsLayerName);
        currentCollisionLayer = LayerMask.GetMask(physicsLayerName);
    }
}