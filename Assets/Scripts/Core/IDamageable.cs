using UnityEngine;

public struct DamageData
{
    public float Amount;
    public Vector2 HitDirection;
    public GameObject Source;
    public float KnockbackForce;
    public float KnockbackDuration;
}

public interface IDamageable
{
    void TakeDamage(DamageData damageData);
}
