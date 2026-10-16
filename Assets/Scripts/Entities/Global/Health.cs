using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    [Header("Events")]
    [Tooltip("Fires whenever damage is taken. Passes current HP and max HP.")]
    public UnityEvent<float, float> OnHealthChanged;

    [Tooltip("Fires when damage is successfully applied.")]
    public UnityEvent<float> OnDamaged;

    public UnityEvent OnDeath;
    private bool isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        if (OnHealthChanged != null)
        {
            OnHealthChanged.Invoke(CurrentHealth, maxHealth);
        }
    }

    public void SetMaxHealth(float newMaxHealth, bool resetCurrentHealth = true)
    {
        maxHealth = newMaxHealth;

        if (resetCurrentHealth)
        {
            CurrentHealth = maxHealth;
        }
        else
        {
            CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
        }

        if (OnHealthChanged != null)
        {
            OnHealthChanged.Invoke(CurrentHealth, maxHealth);
        }
    }

    public void ApplyDamage(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0f);

        // Notify anything listening (for UI, damage popups, hit flashes etc)
        if (OnHealthChanged != null)
        {
            OnHealthChanged.Invoke(CurrentHealth, maxHealth);
        }

        if (OnDamaged != null)
        {
            OnDamaged.Invoke(amount);
        }

        if (CurrentHealth <= 0f && !isDead)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;

        if (OnDeath != null)
        {
            OnDeath.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);

        if (OnHealthChanged != null)
        {
            OnHealthChanged.Invoke(CurrentHealth, maxHealth);
        }
    }

    public void ResetHealth()
    {
        CurrentHealth = maxHealth;
        isDead = false;

        if (OnHealthChanged != null)
        {
            OnHealthChanged.Invoke(CurrentHealth, maxHealth);
        }
    }
}
