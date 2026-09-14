using System.Collections;
using UnityEngine;

public class TrainingDummy : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private KnockbackReceiver knockbackReceiver;
    [SerializeField] private HitFeedback hitFeedback;

    [Header("Dummy Reset Timer")]
    [SerializeField] private float resetDelay = 3f; 

    private Coroutine resetDummy;
    private float remainingResetTime;

    public float RemainingResetTime => remainingResetTime;
    public float ResetDelay => resetDelay;

    public void Awake()
    {
        health = GetComponent<Health>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        hitFeedback = GetComponent<HitFeedback>();
    }

    public void TakeDamage(DamageData payload)
    {
        if (health != null)
        {
            health.ApplyDamage(payload.Amount);
        }

        if (knockbackReceiver != null)
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
