using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private DamageData damageData;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    public void Initialize(DamageData data, float activeHitDuration)
    {
        this.damageData = data;

        Invoke(nameof(DisableHitbox), activeHitDuration);

        float vfxDuration = GetAnimationDuration();

        Destroy(gameObject, vfxDuration);
    }

    private float GetAnimationDuration()
    {
        if (TryGetComponent<Animator>(out var animator))
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.length > 0f)
            {
                return stateInfo.length;
            }
        }

        return 0.3f; // if no animator is attached
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (damageData.Source == null)
        {
            return;
        }

        GameObject rootTarget = collision.transform.root.gameObject;

        if (rootTarget != damageData.Source && !hitTargets.Contains(rootTarget))
        {
            if (collision.TryGetComponent<IDamageable>(out var damageable))
            {
                hitTargets.Add(rootTarget);
                damageable.TakeDamage(damageData);
            }
        }
    }

    private void DisableHitbox()
    {
        if (TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (TryGetComponent<Collider2D>(out var col))
        {
            Color colour = Gizmos.color;
            if (col.enabled)
            {
                colour = Color.red;
            }
            else
            {
                colour = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            }

            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider2D box)
            {
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(circle.offset, circle.radius);
            } else if (col is PolygonCollider2D poly) 
            {
                for (int i = 0; i < poly.pathCount; i++)
                {
                    Vector2[] points = poly.GetPath(i);
                    for (int j = 0; j < points.Length; j++)
                    {
                        Vector2 start = points[j] + poly.offset;
                        Vector2 end = points[(j + 1) % points.Length] + poly.offset;
                        Gizmos.DrawLine(start, end);
                    }
                }
            }
        }
    }
}
