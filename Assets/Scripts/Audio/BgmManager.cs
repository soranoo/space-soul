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

    [Header("Clips")]
    [SerializeField] private AudioClip normalClip;
    [SerializeField] private AudioClip lowHealthClip;

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

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float targetVolume = 0.8f;

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
        currentState = BgmState.Normal;
        pendingState = currentState;
        activeSource.clip = normalClip;
        activeSource.volume = targetVolume;
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
        AudioClip nextClip = nextState == BgmState.LowHealth ? lowHealthClip : normalClip;
        if (nextClip == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        inactiveSource.clip = nextClip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        fadeRoutine = StartCoroutine(Crossfade(activeSource, inactiveSource, fadeDuration));

        currentState = nextState;
        nextSwitchAllowedTime = Time.time + minClipPlaySeconds;

        // Swap roles
        AudioSource temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
    }

    private IEnumerator Crossfade(AudioSource from, AudioSource to, float duration)
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
                to.volume = Mathf.Lerp(startTo, targetVolume, t);
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
            to.volume = targetVolume;
        }

        fadeRoutine = null;
    }
}
