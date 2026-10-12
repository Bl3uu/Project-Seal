using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent (typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EntityMotor : MonoBehaviour
{
    [Header("Elevation Settings")]
    [Tooltip("Global Elevation settings asset)")]
    [SerializeField] private ElevationConfig elevationConfig;
    [SerializeField] private int defaultHeight = 0;

    private LayerMask activeObstacleMask;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    public float MoveSpeed { get; set; } = 5f;
    public float StairYBias { get; set; } = 0f;
    public Vector2 MoveDirection { get; private set; }

    public bool IsMoving => MoveDirection != Vector2.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        UpdateHeightLevel(defaultHeight);
    }

    public void Move(Vector2 direction, float speed)
    {
        MoveDirection = direction;
        MoveSpeed = speed;

        if (direction == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (Mathf.Abs(direction.x) > 0.01f && StairYBias != 0f)
        {
            direction.y += Mathf.Sign(direction.x) * StairYBias;
        }

        Vector2 moveVec = direction;

        RaycastHit2D hit = CheckCollision(moveVec.normalized);

        if (hit.collider != null)
        {

            moveVec -= Vector2.Dot(moveVec, hit.normal) * hit.normal;
        }

        rb.linearVelocity = moveVec.normalized * speed;
    }

    public RaycastHit2D CheckCollision(Vector2 dir)
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
            activeObstacleMask
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


    public void UpdateHeightLevel(int heightLevel)
    {
        if (elevationConfig == null)
        {
            Debug.LogWarning($"[{gameObject.name} No ElevationConfig assigned in PlayerMovement Inspector]");
            return;
        }

        activeObstacleMask = elevationConfig.GetMaskForHeight(heightLevel);
        Debug.Log($"[{gameObject.name}] Switch to Height Level {heightLevel} (Active Mask: {LayerMask.LayerToName((int)Mathf.Log(activeObstacleMask.value, 2))})");
    }
}
