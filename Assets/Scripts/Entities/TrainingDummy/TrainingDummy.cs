using System.Collections;
using UnityEngine;

public class TrainingDummy : Enemy
{
    [Header("Dummy Reset Timer")]
    [SerializeField] private float resetDelay = 3f; 

    private Coroutine resetDummy;
    private float remainingResetTime;

    public float RemainingResetTime => remainingResetTime;
    public float ResetDelay => resetDelay;

    public override void TakeDamage(DamageData payload)
    {
        float currentHp;

        if (health != null)
        {
            currentHp = health.CurrentHealth;
        }
        else
        {
            currentHp = 0f;
        }

        if (health != null && (currentHp - payload.Amount) <= 0f)
        {
            float adjustDamage = Mathf.Max(0f, currentHp - 1f);
                health.ApplyDamage(adjustDamage);
        }
        else
        {
            base.TakeDamage(payload);
        }

        if (knockbackReceiver != null && payload.KnockbackForce > 0f)
        {
            knockbackReceiver.ApplyKnockback(payload.HitDirection, payload.KnockbackForce, payload.KnockbackDuration);
        }

        if (hitFeedback != null)
        {
            hitFeedback.PlayHitEffects();
        }

        if (resetDummy != null)
        {
            StopCoroutine(resetDummy);
        }

        resetDummy = StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        remainingResetTime = resetDelay;

        while (remainingResetTime > 0f)
        {
            yield return null;
            remainingResetTime -= Time.deltaTime;
        }

        remainingResetTime = 0f;
  
        if (health != null)
        {
            health.ResetHealth();
        }
    }
}
