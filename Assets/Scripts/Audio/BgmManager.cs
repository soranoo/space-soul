using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles gameplay BGM with smooth crossfades between normal and low-health tracks.
/// Enforces a minimum play duration before switching to prevent rapid toggles.
/// </summary>
public class BgmManager : MonoBehaviour
{
    public enum BgmState
    {
        Normal,
        LowHealth
    }

    [Header("Settings")]
    [SerializeField] private AudioSettings normalBgmSettings;
    [SerializeField] private AudioSettings lowHealthBgmSettings;

    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    [Header("Thresholds")]
    [Range(0.05f, 1f)]
    [SerializeField] private float lowHealthThresholdPercent = 0.3f;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float minClipPlaySeconds = 10f;
    [SerializeField] private bool playOnStart = true;

    private AudioSource primarySource;
    private AudioSource secondarySource;
    private AudioSource activeSource;
    private AudioSource inactiveSource;

    private BgmState currentState = BgmState.Normal;
    private BgmState pendingState = BgmState.Normal;
    private float nextSwitchAllowedTime;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        primarySource = GetComponent<AudioSource>();
        if (primarySource == null)
        {
            primarySource = gameObject.AddComponent<AudioSource>();
        }

        secondarySource = gameObject.AddComponent<AudioSource>();

        ConfigureSource(primarySource);
        ConfigureSource(secondarySource);

        activeSource = primarySource;
        inactiveSource = secondarySource;
    }

    private void OnEnable()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        if (player != null)
        {
            player.HealthChanged += OnHealthChanged;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayInitial();
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.HealthChanged -= OnHealthChanged;
        }
    }

    private void Update()
    {
        if (pendingState != currentState && Time.time >= nextSwitchAllowedTime)
        {
            SwitchTo(pendingState);
        }
    }

    private void ConfigureSource(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = true;
        source.volume = 0f;
        if (bgmMixerGroup != null)
        {
            source.outputAudioMixerGroup = bgmMixerGroup;
        }
    }

    private void PlayInitial()
    {
        if (normalBgmSettings == null || normalBgmSettings.Clip == null)
        {
            return;
        }

        currentState = BgmState.Normal;
        pendingState = currentState;

        ApplySettingsToSource(activeSource, normalBgmSettings);
        activeSource.clip = normalBgmSettings.Clip;
        activeSource.volume = normalBgmSettings.Source != null ? normalBgmSettings.Source.Volume : 1f;
        activeSource.Play();
        nextSwitchAllowedTime = Time.time + minClipPlaySeconds;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (max <= 0)
        {
            return;
        }

        float percent = (float)current / max;
        BgmState desired = percent <= lowHealthThresholdPercent ? BgmState.LowHealth : BgmState.Normal;

        RequestState(desired);
    }

    private void RequestState(BgmState desiredState)
    {
        pendingState = desiredState;

        if (currentState == desiredState)
        {
            return;
        }

        if (Time.time >= nextSwitchAllowedTime)
        {
            SwitchTo(desiredState);
        }
    }

    private void SwitchTo(BgmState nextState)
    {
        AudioSettings nextSettings = nextState == BgmState.LowHealth ? lowHealthBgmSettings : normalBgmSettings;
        if (nextSettings == null || nextSettings.Clip == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        ApplySettingsToSource(inactiveSource, nextSettings);
        inactiveSource.clip = nextSettings.Clip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float targetVolume = nextSettings.Source != null ? nextSettings.Source.Volume : 1f;
        fadeRoutine = StartCoroutine(Crossfade(activeSource, inactiveSource, fadeDuration, targetVolume));

        currentState = nextState;
        nextSwitchAllowedTime = Time.time + minClipPlaySeconds;

        // Swap roles
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float duration, float toTargetVolume)
    {
        float elapsed = 0f;
        float startFrom = from != null ? from.volume : 0f;
        float startTo = to != null ? to.volume : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);

            if (from != null)
            {
                from.volume = Mathf.Lerp(startFrom, 0f, t);
            }

            if (to != null)
            {
                to.volume = Mathf.Lerp(startTo, toTargetVolume, t);
            }

            yield return null;
        }

        if (from != null)
        {
            from.Stop();
            from.volume = 0f;
        }

        if (to != null)
        {
            to.volume = toTargetVolume;
        }

        fadeRoutine = null;
    }

    private void ApplySettingsToSource(AudioSource source, AudioSettings settings)
    {
        if (source == null || settings == null)
        {
            return;
        }

        if (settings.Source != null)
        {
            settings.Source.ApplyTo(source);

            if (settings.Source.MixerGroup == null && bgmMixerGroup != null)
            {
                source.outputAudioMixerGroup = bgmMixerGroup;
            }
        }
    }
}
