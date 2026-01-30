using UnityEngine;

/// <summary>
/// Swaps the player ship sprite based on current health.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerDamageSprite : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Damage Sprites")]
    [Tooltip("Full health sprite.")]
    [SerializeField] private Sprite fullHealthSprite;

    [Tooltip("Slight damage sprite (high health).")]
    [SerializeField] private Sprite slightDamageSprite;

    [Tooltip("Damaged sprite (medium health).")]
    [SerializeField] private Sprite damagedSprite;

    [Tooltip("Very damaged sprite (low health).")]
    [SerializeField] private Sprite veryDamagedSprite;

    [Header("Thresholds")]
    [Tooltip("Health percentage threshold for full health sprite.")]
    [Range(0f, 1f)]
    [SerializeField] private float fullHealthThreshold = 0.75f;

    [Tooltip("Health percentage threshold for slight damage sprite.")]
    [Range(0f, 1f)]
    [SerializeField] private float slightDamageThreshold = 0.5f;

    [Tooltip("Health percentage threshold for damaged sprite.")]
    [Range(0f, 1f)]
    [SerializeField] private float damagedThreshold = 0.25f;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }
    }

    private void OnEnable()
    {
        Subscribe();
        RefreshImmediate();
    }

    private void OnDisable()
    {
        Unsubscribe();
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

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float percent = maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;

        if (percent >= fullHealthThreshold)
        {
            SetSprite(fullHealthSprite);
        }
        else if (percent >= slightDamageThreshold)
        {
            SetSprite(slightDamageSprite);
        }
        else if (percent >= damagedThreshold)
        {
            SetSprite(damagedSprite);
        }
        else
        {
            SetSprite(veryDamagedSprite);
        }
    }

    private void SetSprite(Sprite sprite)
    {
        if (sprite == null)
        {
            return;
        }

        if (spriteRenderer.sprite != sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
