using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private Poise poise;

    [Header("UI Fills")]
    [SerializeField] private Image healthFill;
    [SerializeField] private Image poiseFill;

    private void OnEnable()
    {
        if (health != null)
        {
            health.OnHealthChanged.AddListener(UpdateHealthBar);
        }
        if (poise != null)
        {
            poise.OnPoiseChanged.AddListener(UpdatePoiseBar);
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
        if (poise != null)
        {
            poise.OnPoiseChanged.RemoveListener(UpdatePoiseBar);
        }
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }

    private void UpdatePoiseBar(float currentPoise, float maxPoise)
    {
        if (poiseFill != null)
        {
            poiseFill.fillAmount = Mathf.Clamp01(currentPoise / maxPoise);
        }
    }
}
