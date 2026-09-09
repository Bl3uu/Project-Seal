using UnityEngine;

public struct DamageData
{
    public float Amount;
    public Vector2 HitDirection;
    public GameObject Source;
}

public interface IDamageable
{
    void TakeDamage(DamageData damageData);
}
