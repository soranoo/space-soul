using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Pulses the vignette when the player's health drops below a threshold.
/// Attach this to the gameplay camera.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class PlayerHealthVignetteController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private Volume volume;

    [Header("Low Health Pulse")]
    [Range(0f, 1f)]
    [SerializeField] private float healthThreshold = 0.35f;

    [SerializeField] private Color targetColor = new Color(0.75f, 0.05f, 0.05f, 1f);

    [Min(0f)]
    [SerializeField] private float pulseSpeed = 2f;

    [Range(0f, 1f)]
    [SerializeField] private float targetIntensity = 0.32f;

    private Vignette vignette;
    private Color originalColor;
    private float originalIntensity;
    private bool isPulsing;

    private void Awake()
    {
        ResolvePlayer();
        ResolveVolume();
        CacheVignette();
    }

    private void OnEnable()
    {
        ResolvePlayer();
        ResolveVolume();
        CacheVignette();
        Subscribe();
        RefreshImmediate();
    }

    private void OnDisable()
    {
        Unsubscribe();
        RestoreOriginal();
    }

    private void Update()
    {
        if (!isPulsing || vignette == null)
        {
            return;
        }

        float pulse = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;
        vignette.color.value = Color.Lerp(originalColor, targetColor, pulse);
        vignette.intensity.value = Mathf.Lerp(originalIntensity, targetIntensity, pulse);
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

    private void ResolvePlayer()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
    }

    private void ResolveVolume()
    {
        if (volume != null)
        {
            return;
        }

        volume = GetComponent<Volume>();

        if (volume != null)
        {
            return;
        }

        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);

        foreach (Volume candidate in volumes)
        {
            if (!candidate.isGlobal || candidate.sharedProfile == null)
            {
                continue;
            }

            if (candidate.sharedProfile.TryGet(out Vignette _))
            {
                volume = candidate;
                return;
            }
        }
    }

    private void CacheVignette()
    {
        if (volume == null || volume.profile == null)
        {
            vignette = null;
            return;
        }

        if (!volume.profile.TryGet(out vignette))
        {
            vignette = null;
            return;
        }

        vignette.color.overrideState = true;
        vignette.intensity.overrideState = true;

        originalColor = vignette.color.value;
        originalIntensity = vignette.intensity.value;
    }

    private void RefreshImmediate()
    {
        if (player == null)
        {
            RestoreOriginal();
            return;
        }

        int maxHealth = player.Stats != null ? player.Stats.MaxHealth : 0;
        OnHealthChanged(player.CurrentHealth, maxHealth);
    }

    private void OnHealthChanged(int currentHealth, int maxHealth)
    {
        if (vignette == null || maxHealth <= 0)
        {
            RestoreOriginal();
            return;
        }

        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);

        if (healthPercent <= healthThreshold)
        {
            isPulsing = true;
            return;
        }

        RestoreOriginal();
    }

    private void RestoreOriginal()
    {
        isPulsing = false;

        if (vignette == null)
        {
            return;
        }

        vignette.color.value = originalColor;
        vignette.intensity.value = originalIntensity;
    }
}