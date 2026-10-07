using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    private DamageData damageData;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();

    public void Initialize(DamageData data, float lifetime)
    {
        this.damageData = data;
        Destroy(gameObject, lifetime);
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
}
