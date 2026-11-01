using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class EnemyKnockbackState : IEnemyState
{
    private readonly Enemy enemy;
    private readonly EntityMotor motor;
    private readonly Rigidbody2D rb;

    private Vector2 slideDirection;
    private float initialForce;
    private float currentDuration;
    private float knockbackTimer;
    private bool hasCollidedThisKnockback = false;
    private const float SpeedMultiplier = 2.5f;
    private bool hasBowlingRelic = false;

    public EnemyKnockbackState(Enemy enemy, EntityMotor motor, Rigidbody2D rb)
    {
        this.enemy = enemy;
        this.motor = motor;
        this.rb = rb;
    }

    public void SetupKnockback(Vector2 direction, float force, float duration)
    {
        slideDirection = direction.normalized;
        initialForce = force;
        currentDuration = duration;
    }

    public void Enter()
    {
        knockbackTimer = 0f;
        hasCollidedThisKnockback = false;

        if (motor != null)
        {
            motor.IsKnockedBack = true;
        }
    }

    public void UpdateState()
    {
        knockbackTimer += Time.deltaTime;

        if (knockbackTimer >= currentDuration)
        {
            enemy.ChangeState(null);
        }
    }

    public void FixedUpdateState()
    {
        if (rb != null && currentDuration > 0f)
        {
            float progress = Mathf.Clamp01(knockbackTimer / currentDuration);
            float currentSpeed = Mathf.Lerp(initialForce, 0f, progress);
            rb.linearVelocity = slideDirection * currentSpeed;
        }
    }

    public void Exit()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (motor != null)
        {
            motor.IsKnockedBack = false;
        }
    }

    public void OnImpactCollision(Collision2D collision)
    {
        if (hasCollidedThisKnockback)
        {
            return;
        }

        GameObject hitObject = collision.gameObject;

        // Enemy to Enemy Collision
        if (collision.collider.TryGetComponent<IKnockbackable>(out var targetKnockbackable) && hitObject != enemy.gameObject)
        {
            hasCollidedThisKnockback = true;
            HandleEnemyImpact(collision, targetKnockbackable);
            return;
        }

        // Enemy to Prop/Pillar/Barrel etc Collision
        int destructibleLayer = LayerMask.NameToLayer("Destructible");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        bool hasDamageable = collision.collider.TryGetComponent<IDamageable>(out var damageableProp);
        bool isNotEnemy = hitObject.layer != enemyLayer;

        if (hitObject.layer == destructibleLayer || (hasDamageable && isNotEnemy))
        {
            hasCollidedThisKnockback = true;
            HandlePropImpact(collision, damageableProp);
            return;
        }

        // Enemy to Wall/Environment Collision
        int obstacleLayer = LayerMask.NameToLayer("Obstacle");
        if (hitObject.layer == obstacleLayer)
        {
            hasCollidedThisKnockback= true;
            HandleWallImpact(collision);
            return;
        }
    }

    private float CalculateImpactDamage(float baseDamage, float impactSpeed)
    {
        return baseDamage + (impactSpeed * SpeedMultiplier);
    }

    private void HandleWallImpact(Collision2D collision)
    {
        Debug.Log($"[Knockback Impact] {enemy.name} slammed into Wall/Environment", enemy);

        float impactSpeed;

        if (rb != null)
        {
            impactSpeed = rb.linearVelocity.magnitude;
        }
        else
        {
            impactSpeed = initialForce;
        }

        float finalImpactDamage = CalculateImpactDamage(baseDamage: 5f, impactSpeed);

        Debug.Log($"[Kinetic Impact] Wall hit at speed {impactSpeed:F1}! Dealt {finalImpactDamage:F1} damage.");

        DamageData wallSlamDamage = new DamageData();

        wallSlamDamage.Amount = finalImpactDamage;
        wallSlamDamage.PoiseDamage = 15f;
        wallSlamDamage.HitDirection = -slideDirection;

        enemy.TakeDamage(wallSlamDamage);
        enemy.ChangeState(null);
    }

    private void HandleEnemyImpact(Collision2D collision, IKnockbackable secondaryTarget)
    {
        Debug.Log($"[Knockback Impact] {enemy.name} crash into another enemy: {collision.gameObject.name}", enemy);

        float impactSpeed;

        if (rb != null)
        {
            impactSpeed = rb.linearVelocity.magnitude;
        }
        else
        {
            impactSpeed = initialForce;
        }

        float finalCollateralDamage = CalculateImpactDamage(baseDamage: 8f, impactSpeed);

        Debug.Log($"[Kinetic Impact] {enemy.name} crashed into {collision.gameObject.name} at speed {impactSpeed:F1}! Dealt {finalCollateralDamage:F1} damage", enemy);

        if (collision.collider.TryGetComponent<IDamageable>(out var secondaryDamageable))
        {
            DamageData collateralPayload = new DamageData();
            collateralPayload.Amount = finalCollateralDamage;
            collateralPayload.PoiseDamage = 10f;
            collateralPayload.HitDirection = slideDirection;
            collateralPayload.KnockbackForce = 0f;
            collateralPayload.KnockbackDuration = 0f;

            secondaryDamageable.TakeDamage(collateralPayload);
        }

        if (hasBowlingRelic && secondaryTarget != null)
        {
            float transferredForce = initialForce * 0.7f;
            float tranferredDuration = currentDuration * 0.8f;

            secondaryTarget.ApplyKnockback(slideDirection, transferredForce, tranferredDuration);
            Debug.Log($"[Relic Synergy] Bowling transfer applied to {collision.gameObject.name} with force {transferredForce:F1}!", enemy);
        }

        DamageData selfCrashPayload = new DamageData();
        selfCrashPayload.Amount = finalCollateralDamage * 0.5f;
        enemy.TakeDamage(selfCrashPayload);
        enemy.ChangeState(null);
    }

    private void HandlePropImpact(Collision2D collision, IDamageable prop)
    {
        Debug.Log($"[Knockback Impact] {enemy.name} smashed into Prop: {collision.gameObject.name}", enemy);

        float impactSpeed;

        if (rb != null)
        {
            impactSpeed = rb.linearVelocity.magnitude;
        }
        else
        {
            impactSpeed = initialForce;
        }

        float finalPropDamage = CalculateImpactDamage(baseDamage: 12f, impactSpeed);

        Debug.Log($"[Kinetic Impact] {enemy.name} smashed into Prop {collision.gameObject.name} at speed {impactSpeed:F1}! Dealt {finalPropDamage:F1} damage", enemy);

        DamageData propPayload = new DamageData();
        propPayload.Amount = finalPropDamage;
        propPayload.HitDirection = slideDirection;
        propPayload.KnockbackForce = initialForce * 0.5f;

        prop.TakeDamage(propPayload);

        DamageData recoilPayload = new DamageData();
        recoilPayload.Amount = 4f;
        enemy.TakeDamage(recoilPayload);

        enemy.ChangeState(null);
    }
}
