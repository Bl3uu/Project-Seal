using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;

    public float CurrentHealth { get; private set; }
    public float MaxHeatlh => maxHealth;

    [Header("Events")]
    [Tooltip("Fires whenever damage is taken. Passes current HP and max HP.")]
    public UnityEvent<float, float> OnHealthChanged;

    [Tooltip("Fires on the exact hit that reduces health to 0.")]
    public UnityEvent<DamageData> OnTakeDamage;

    public UnityEvent OnDeath;
    private bool isDead;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    private void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(DamageData damageData)
    {
        if (isDead || damageData.Amount <= 0f)
        {
            return;
        }

        CurrentHealth -= damageData.Amount;
        CurrentHealth = Mathf.Max(CurrentHealth, 0f);

        // Notify anything listening (for UI, damage popups, hit flashes etc)
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        OnTakeDamage?.Invoke(damageData);

        if (CurrentHealth <= 0f && !isDead)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
        {
            return;
        }

        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
