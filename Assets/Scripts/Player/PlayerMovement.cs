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
    private BoxCollider2D boxCollider;
    private DashController dashController;
    private Vector2 moveDirection;

    public float StairYBias { get; set; } = 0f;

    public Vector2 MoveDirection => moveDirection;
    public bool IsMoving => moveDirection != Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
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

        return new Vector2(x, y);
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

        Vector2 moveVec = direction;

        // Cast the BoxCast in actual movement direction and not split into X/Y
        RaycastHit2D hit = CheckCollision(moveVec.normalized);

        if (hit.collider != null)
        {
            // Will now redirect movement along any angle
            moveVec = moveVec - Vector2.Dot(moveVec, hit.normal) * hit.normal;
        }

        rb.linearVelocity = moveVec.normalized * moveSpeed;
    }

    private RaycastHit2D CheckCollision(Vector2 dir)
    {
        if (boxCollider == null)
        {
            return new RaycastHit2D();
        }

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            boxCollider.bounds.center,
            boxCollider.size,
            0f,
            dir,
            0.15f,
            currentCollisionLayer
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
            {
                return hit;
            }
        }

        return new RaycastHit2D();
    }

    public void UpdateCollisionLayer(string physicsLayerName)
    {
        gameObject.layer = LayerMask.NameToLayer(physicsLayerName);
        currentCollisionLayer = LayerMask.GetMask(physicsLayerName);
    }
}