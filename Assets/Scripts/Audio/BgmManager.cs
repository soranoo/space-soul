using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Handles BGM playback and crossfades between requested tracks.
/// Track selection is driven by external systems.
/// </summary>
public class BgmManager : SingletonBase<BgmManager>
{
    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float minClipPlaySeconds = 10f;

    private AudioSource primarySource;
    private AudioSource secondarySource;
    private AudioSource activeSource;
    private AudioSource inactiveSource;

    private AudioSettings currentTrackSettings;
    private AudioSettings pendingTrackSettings;
    private float nextSwitchAllowedTime;
    private Coroutine fadeRoutine;
    private GameStateMachine observedStateMachine;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

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
        TrySubscribeToStateChanges();
    }

    private void Start()
    {
        TrySubscribeToStateChanges();
    }

    private void OnDisable()
    {
        UnsubscribeFromStateChanges();
    }

    private void Update()
    {
        if (observedStateMachine == null)
        {
            TrySubscribeToStateChanges();
        }

        if (pendingTrackSettings != null && pendingTrackSettings != currentTrackSettings && Time.time >= nextSwitchAllowedTime)
        {
            SwitchTo(pendingTrackSettings);
        }
    }

    private void TrySubscribeToStateChanges()
    {
        if (observedStateMachine != null)
        {
            return;
        }

        GameManager gameManager = GameManager.Instance;
        if (gameManager == null || gameManager.StateMachine == null)
        {
            return;
        }

        observedStateMachine = gameManager.StateMachine;
        observedStateMachine.StateChanged += HandleGameStateChanged;
    }

    private void UnsubscribeFromStateChanges()
    {
        if (observedStateMachine == null)
        {
            return;
        }

        observedStateMachine.StateChanged -= HandleGameStateChanged;
        observedStateMachine = null;
    }

    private void HandleGameStateChanged(IGameState previousState, IGameState newState)
    {
        nextSwitchAllowedTime = 0f;
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

    public void RequestTrack(AudioSettings trackSettings)
    {
        if (trackSettings == null || trackSettings.Clip == null)
        {
            return;
        }

        pendingTrackSettings = trackSettings;

        if (currentTrackSettings == trackSettings)
        {
            return;
        }

        if (Time.time >= nextSwitchAllowedTime)
        {
            SwitchTo(trackSettings);
        }
    }

    public void SetTrackImmediate(AudioSettings trackSettings)
    {
        if (trackSettings == null || trackSettings.Clip == null)
        {
            return;
        }

        pendingTrackSettings = trackSettings;

        if (currentTrackSettings == trackSettings)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        SwitchTo(trackSettings);
    }

    private void SwitchTo(AudioSettings nextSettings)
    {
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

        currentTrackSettings = nextSettings;
        pendingTrackSettings = nextSettings;
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
