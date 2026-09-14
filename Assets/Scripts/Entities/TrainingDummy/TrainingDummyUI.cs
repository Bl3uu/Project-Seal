using TMPro;
using UnityEngine;

public class TrainingDummyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private TrainingDummy trainingDummy;
    [SerializeField] private TextMeshProUGUI statsText;

    private float totalDamageTaken;
    private float lastDamageReceived;

    private void Awake()
    {
        if (trainingDummy == null)
        {
            trainingDummy = GetComponentInParent<TrainingDummy>();
        }

        if (health == null)
        {
            health = GetComponentInParent<Health>();
        }
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnDamaged.AddListener(HandledDamaged);
            health.OnHealthChanged.AddListener(HandleHealthChanged);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnDamaged.RemoveListener(HandledDamaged);
            health.OnHealthChanged.RemoveListener(HandleHealthChanged);
        }
    }

    private void Update()
    {
        if (trainingDummy != null && trainingDummy.RemainingResetTime > 0f)
        {
            UpdateTextDisplay(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void HandledDamaged(float amount)
    {
        lastDamageReceived = amount;
        totalDamageTaken += amount;

        if (health != null)
        {
            UpdateTextDisplay(health.CurrentHealth, health.MaxHealth);
        }
    }

    private void HandleHealthChanged(float currentHp, float maxHp)
    {
        if (currentHp >= maxHp)
        {
            totalDamageTaken = 0f;
            lastDamageReceived = 0f;
        }

        UpdateTextDisplay(currentHp, maxHp);
    }

    private void UpdateTextDisplay (float currentHp, float maxHp)
    {
        if (statsText == null) return;

        float timer;

        if (trainingDummy != null)
        {
            timer = trainingDummy.RemainingResetTime;
        }
        else
        {
            timer = 0f;
        }

        statsText.text = $"HP: {currentHp}/{maxHp}\n" +
                         $"Last Hit: {lastDamageReceived}\n" +
                         $"Total: {totalDamageTaken}\n" +
                         $"Reset in: {timer:F1}s";
    }
}
