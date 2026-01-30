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

    private void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
    }

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
        if (healthSlider == null)
        {
            return;
        }

        healthSlider.maxValue = max;
        healthSlider.value = current;

        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }
}
