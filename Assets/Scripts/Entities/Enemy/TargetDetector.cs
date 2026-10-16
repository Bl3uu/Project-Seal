using UnityEngine;

public class TargetDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Enemy enemy;
    
    [Header("Detection Settings")]
    [SerializeField] private LayerMask targetLayer;
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

            return (currentTarget.position - transform.position).normalized;
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

            return Vector2.Distance(transform.position, currentTarget.position);
        }
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
