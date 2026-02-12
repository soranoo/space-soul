using UnityEngine;

/// <summary>
/// Maps player health changes to BGM track transitions.
/// Keeps player-specific rules out of BgmManager.
/// </summary>
public class PlayerBgmAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    private BgmManager bgmManager;

    [Header("BGM Settings")]
    [SerializeField] private AudioSettings normalBgmSettings;
    [SerializeField] private AudioSettings lowHealthBgmSettings;

    [Header("Thresholds")]
    [Range(0.05f, 1f)]
    [SerializeField] private float lowHealthThresholdPercent = 0.3f;

    private void Awake()
    {
        bgmManager = BgmManager.Instance;
    }

    private void OnEnable()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }

        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            player.HealthChanged += OnHealthChanged;

            if (player.CurrentHealth > 0)
            {
                OnHealthChanged(player.CurrentHealth, player.Stats != null ? player.Stats.MaxHealth : 0);
            }
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (max <= 0 || bgmManager == null)
        {
            return;
        }

        float percent = (float)current / max;
        AudioSettings desiredTrack = percent <= lowHealthThresholdPercent ? lowHealthBgmSettings : normalBgmSettings;
        bgmManager.RequestTrack(desiredTrack);
    }
}
