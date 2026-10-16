using UnityEngine;

public class EnemyAim : MonoBehaviour, IAimProvider
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform weaponPivot;

    public Vector2 AimDirection { get; private set; } = Vector2.right;
    public Vector2 AimWorldPosition => (Vector2)transform.position + AimDirection * 2f;

    public void UpdateAim(Vector2 targetDirection)
    {
        if (targetDirection.sqrMagnitude < 0.01f)
        {
            return;
        }

        AimDirection = targetDirection.normalized;

        if (spriteRenderer != null)
        {
            if (AimDirection.x < -0.1f)
            {
                spriteRenderer.flipX = true;
            }
            else if (AimDirection.x > 0.1f)
            {
                spriteRenderer.flipX = false;
            }
        }

        if (weaponPivot != null)
        {
            float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
            weaponPivot.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
