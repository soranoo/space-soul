using UnityEngine;

/// <summary>
/// Maps player health changes to BGM pitch changes.
/// Keeps player-specific rules out of BgmManager.
/// </summary>
public class PlayerBgmAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;
    private BgmManager bgmManager;

    [Header("BGM Settings")]
    [SerializeField] private AudioSettings bgmSettings;

    [Header("Health Pitch")]
    [SerializeField] private AnimationCurve healthToPitchCurve = new AnimationCurve(
        new Keyframe(0f, 0.85f),
        new Keyframe(1f, 1f));

    [Header("Death Pitch")]
    [SerializeField] private float deadPitchMultiplier = 0.7f;
    [Min(0f)]
    [SerializeField] private float deathPitchTransitionSeconds = 0.4f;

    private bool isPlayerDead;

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
            isPlayerDead = false;
            player.HealthChanged += OnHealthChanged;
            player.Died += OnPlayerDied;

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
            player.Died -= OnPlayerDied;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (max <= 0 || bgmManager == null || isPlayerDead)
        {
            return;
        }

        float percent = Mathf.Clamp01((float)current / max);

        if (bgmSettings != null)
        {
            bgmManager.RequestTrack(bgmSettings);
        }

        float pitchMultiplier = healthToPitchCurve != null ? healthToPitchCurve.Evaluate(percent) : 1f;
        bgmManager.SetRuntimePitchMultiplier(pitchMultiplier);
    }

    private void OnPlayerDied()
    {
        if (bgmManager == null)
        {
            return;
        }

        isPlayerDead = true;
        bgmManager.SetRuntimePitchMultiplier(deadPitchMultiplier, deathPitchTransitionSeconds);
    }
}
