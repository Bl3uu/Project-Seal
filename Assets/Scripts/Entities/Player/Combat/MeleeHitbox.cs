using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private DamageData damageData;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private Collider2D hitboxCollider;
    private Animator animator;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    public void PrepareHitbox(DamageData data)
    {
        this.damageData = data;
        DisableHitbox();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            float clipLength = 0.2f;

            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                clipLength = clipInfo[0].clip.length;
            }
        }
    }

    public void EnableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
        }
    }

    public void DisableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
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
