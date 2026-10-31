using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemy;
    
    [Header("Detection Settings")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Transform currentTarget;

    public Transform CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public Vector2 DirectionToTarget
    {
        get
        {
            if (!HasTarget)
            {
                return Vector2.zero;
            }

            return (GetCenterPosition(currentTarget) - GetSelfCenterPosition()).normalized;
        }
    }

    public float DistanceToTarget
    {
        get
        {
            if (!HasTarget)
            {
                return float.MaxValue;
            }

            return Vector2.Distance(GetSelfCenterPosition(), GetCenterPosition(currentTarget));
        }
    }

    public bool HasLineOfSight()
    {
        if (!HasTarget)
        {
            return false;
        }

        Vector2 startPos = GetSelfCenterPosition();
        Vector2 targetPos = GetCenterPosition(currentTarget);
        Vector2 direction = targetPos - startPos;
        float distance = direction.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction.normalized, distance, obstacleMask);

        if (hit.collider != null && hit.transform != currentTarget)
        {
            return false;
        }

        return true;
    }

    public bool DetectTargetInRadius(float radius)
    {
        if (HasTarget && DistanceToTarget <= radius)
        {
            return true;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, targetLayer);
        if (hit != null)
        {
            currentTarget = hit.transform;
            return true;
        }

        return false;
    }

    private Vector2 GetCenterPosition(Transform targetTransform)
    {
        if (targetTransform == null)
        {
            return Vector2.zero;
        }

        return (Vector2)targetTransform.position + new Vector2(0f, 0.4f);
    }

    private Vector2 GetSelfCenterPosition()
    {
        return (Vector2)transform.position + new Vector2(0f, 0.4f);
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }

    private void OnDrawGizmosSelected()
    {
        float sightRadius;

        if (enemy != null && enemy.Data != null)
        {
            sightRadius = enemy.Data.sightRadius;
        }
        else
        {
            sightRadius = 2f;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, sightRadius);

        if (HasTarget && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 0.5f);
        }
    }
}
