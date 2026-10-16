using System;
using System.Linq.Expressions;
using UnityEngine;

public class Poise : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxPoise = 50f;
    [SerializeField] private float poiseRecoveryRate = 10f;
    [SerializeField] private float recoveryDelay = 2f;

    public float CurrentPoise { get; private set;  }
    public float MaxPoise => maxPoise;
    public bool IsBroken { get; private set; }

    // Events for UI, FX or AI state transitions
    public event Action OnPoisedDamaged;
    public event Action OnGuardBroken;
    public event Action OnGuardRecovered;

    public float lastDamageTime;

    private void Awake()
    {
        CurrentPoise = maxPoise;
    }

    public void Initialize(float maxPoiseValue)
    {
        maxPoise = maxPoiseValue;
        CurrentPoise = maxPoise;
        IsBroken = false;
    }

    private void Update()
    {
        if (Time.time >= lastDamageTime + recoveryDelay)
        {
            if (IsBroken)
            {
                RecoveryGuard();
            }
            else if (CurrentPoise < maxPoise)
            {
                CurrentPoise = Mathf.Min(maxPoise, CurrentPoise + poiseRecoveryRate * Time.deltaTime);
            }
        }
    }

    public void ApplyPoiseDamage(float amount)
    {
        if (IsBroken || amount <= 0f)
        {
            return;
        }

        lastDamageTime = Time.time;
        CurrentPoise = Mathf.Max(0f, CurrentPoise - amount);

        if (OnPoisedDamaged != null)
        {
            OnPoisedDamaged.Invoke();
        }

        if (CurrentPoise <= 0f)
        {
            BreakGuard();
        }
    }

    public void BreakGuard()
    {
        IsBroken = true;
        
        if (OnGuardBroken != null)
        {
            OnGuardBroken.Invoke();
        }
    }

    public void RecoveryGuard()
    {
        CurrentPoise = maxPoise;
        IsBroken = false;

        if (OnGuardRecovered != null)
        {
            OnGuardRecovered.Invoke();
        }
    }
}
