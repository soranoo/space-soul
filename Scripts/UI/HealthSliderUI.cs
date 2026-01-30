using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Updates a UI slider based on the player's health changes.
/// </summary>
public class HealthSliderUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image fillImage;

    [Header("Appearance")]
    [SerializeField] private Gradient healthGradient;

    private void OnEnable()
    {
        ResolvePlayer();
        Subscribe();
        RefreshImmediate();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void ResolvePlayer()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
    }

    private void Subscribe()
    {
        if (player != null)
        {
            player.HealthChanged += OnHealthChanged;
        }
    }

    private void Unsubscribe()
    {
        if (player != null)
        {
            player.HealthChanged -= OnHealthChanged;
        }
    }

    private void RefreshImmediate()
    {
        if (player == null)
        {
            return;
        }

        OnHealthChanged(player.CurrentHealth, player.Stats.MaxHealth);
    }

    private void OnHealthChanged(int current, int max)
    {
        healthSlider.maxValue = max;
        healthSlider.value = current;

        healthText.text = $"{current}/{max}";

        if (healthGradient != null)
        {
            float percent = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            fillImage.color = healthGradient.Evaluate(percent);
        }
    }
}
