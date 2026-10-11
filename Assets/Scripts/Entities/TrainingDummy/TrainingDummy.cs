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
        base.TakeDamage(payload);

        if (resetDummy != null)
        {
            StopCoroutine(resetDummy);
        }

        resetDummy = StartCoroutine(ResetRoutine());
    }

    private IEnumerator ResetRoutine()
    {
        remainingResetTime = resetDelay;
        Debug.Log("Dummy Reset");
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
