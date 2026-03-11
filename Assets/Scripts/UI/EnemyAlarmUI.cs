using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Displays a pulsing alarm overlay when a dangerous enemy spawns.
/// Requires a CanvasGroup on the same GameObject for the pulse/fade visuals.
/// A looping AudioSource is managed internally for fade-in and fade-out of the alarm SFX.
/// Subscribes to WaveManager.AlarmEnemySpawned automatically.
/// </summary>
public class EnemyAlarmUI : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Total duration of the alarm display in seconds.")]
    [SerializeField] private float duration = 3f;

    [Tooltip("Number of visual pulses per second.")]
    [SerializeField] private float pulseFrequency = 2f;

    [Header("Fade")]
    [Tooltip("Duration of the fade-in at alarm start (seconds).")]
    [SerializeField] private float fadeInDuration = 0.3f;

    [Tooltip("Duration of the fade-out at alarm end (seconds).")]
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Pulse Alpha")]
    [Tooltip("Minimum alpha value during the pulse oscillation.")]
    [Range(0f, 1f)]
    [SerializeField] private float pulseMinAlpha = 0.5f;

    [Tooltip("Maximum alpha value during the pulse oscillation.")]
    [Range(0f, 1f)]
    [SerializeField] private float pulseMaxAlpha = 1f;

    [Header("Root")]
    [Tooltip("CanvasGroup used for the alarm pulse and fade visuals.")]
    [SerializeField] private CanvasGroup root;

    [Header("Audio")]
    [Tooltip("Alarm SFX asset. The clip will be looped and faded in / out.")]
    [SerializeField] private AudioSettings alarmSfx;

    [Tooltip("Fallback mixer group when Alarm SFX settings do not specify one.")]
    [SerializeField] private AudioMixerGroup alarmMixerGroup;

    [Tooltip("Duration of audio fade-in (seconds).")]
    [SerializeField] private float audioFadeInDuration = 0.3f;

    [Tooltip("Duration of audio fade-out (seconds). Audio will end at the same time as the visual.")]
    [SerializeField] private float audioFadeOutDuration = 0.5f;

    private AudioSource audioSource;
    private Sequence alarmSequence;
    private Tween audioFadeTween;

    private void Awake()
    {
        if (root == null)
        {
            root = GetComponent<CanvasGroup>();
        }

        if (root == null)
        {
            Debug.LogError("EnemyAlarmUI: Root CanvasGroup is not assigned and was not found.");
            enabled = false;
            return;
        }

        root.alpha = 0f;
        root.blocksRaycasts = false;
        root.interactable = false;
        SetRootActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void OnEnable()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.AlarmEnemySpawned += OnAlarmEnemySpawned;
        }
    }

    private void OnDisable()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.AlarmEnemySpawned -= OnAlarmEnemySpawned;
        }

        StopAlarm();
    }

    private void OnAlarmEnemySpawned(Enemy enemy)
    {
        TriggerAlarm();
    }

    /// <summary>
    /// Trigger the alarm with the configured default duration.
    /// </summary>
    public void TriggerAlarm()
    {
        TriggerAlarm(duration);
    }

    /// <summary>
    /// Trigger the alarm for an explicit duration, overriding the default.
    /// </summary>
    /// <param name="overrideDuration">How long the alarm should be active.</param>
    public void TriggerAlarm(float overrideDuration)
    {
        if (root == null)
        {
            return;
        }

        StopAlarm();
        SetRootActive(true);

        // Ensure a full in+out cycle fits inside the requested duration.
        float safeDuration = Mathf.Max(fadeInDuration + fadeOutDuration + 0.01f, overrideDuration);

        PlayAlarmAudio();
        BuildAlarmSequence(safeDuration);
    }

    // -------------------------------------------------------------------------
    // Audio
    // -------------------------------------------------------------------------

    private void PlayAlarmAudio()
    {
        if (alarmSfx == null || alarmSfx.Clip == null)
        {
            return;
        }

        float targetVolume = alarmSfx.Source != null ? alarmSfx.Source.Volume : 1f;

        audioSource.clip = alarmSfx.Clip;
        audioSource.volume = 0f;

        if (alarmSfx.Source != null)
        {
            // Apply pitch settings but leave volume at 0 for fade-in.
            if (alarmSfx.Source.RandomizePitch)
            {
                float minPitch = Mathf.Clamp(
                    Mathf.Min(alarmSfx.Source.PitchMin, alarmSfx.Source.PitchMax), -3f, 3f);
                float maxPitch = Mathf.Clamp(
                    Mathf.Max(alarmSfx.Source.PitchMin, alarmSfx.Source.PitchMax), -3f, 3f);
                audioSource.pitch = Random.Range(minPitch, maxPitch);
            }
            else
            {
                audioSource.pitch = alarmSfx.Source.Pitch;
            }

            if (alarmSfx.Source.MixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = alarmSfx.Source.MixerGroup;
            }
            else if (alarmMixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = alarmMixerGroup;
            }
        }
        else if (alarmMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = alarmMixerGroup;
        }

        audioSource.Play();
        audioFadeTween = audioSource.DOFade(targetVolume, audioFadeInDuration);
    }

    private void FadeOutAudio()
    {
        audioFadeTween?.Kill();
        audioFadeTween = audioSource.DOFade(0f, audioFadeOutDuration)
            .OnComplete(() =>
            {
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
            });
    }

    // -------------------------------------------------------------------------
    // Visuals
    // -------------------------------------------------------------------------

    private void BuildAlarmSequence(float totalDuration)
    {
        float holdTime = totalDuration - fadeInDuration - fadeOutDuration;
        float pulsePeriod = 1f / Mathf.Max(0.01f, pulseFrequency);
        float halfPeriod = pulsePeriod * 0.5f;
        int pulseCount = Mathf.Max(1, Mathf.RoundToInt(holdTime / pulsePeriod));

        // Schedule audio fade-out so it ends at the same time as the visual fade-out.
        float pulseTime = pulseCount * pulsePeriod;
        float audioFadeOutStart = Mathf.Max(0f, fadeInDuration + pulseTime + fadeOutDuration - audioFadeOutDuration);

        alarmSequence = DOTween.Sequence();

        // 1 — Fade in
        alarmSequence.Append(
            root.DOFade(pulseMaxAlpha, fadeInDuration));

        // 2 — Pulse (finite loop so the main sequence can advance)
        Sequence pulseSeq = DOTween.Sequence();
        pulseSeq.Append(root.DOFade(pulseMinAlpha, halfPeriod).SetEase(Ease.InOutSine));
        pulseSeq.Append(root.DOFade(pulseMaxAlpha, halfPeriod).SetEase(Ease.InOutSine));
        pulseSeq.SetLoops(pulseCount);
        alarmSequence.Append(pulseSeq);

        // 3 — Kick off the audio fade-out at the right moment
        alarmSequence.InsertCallback(audioFadeOutStart, FadeOutAudio);

        // 4 — Visual fade-out
        alarmSequence.Append(
            root.DOFade(0f, fadeOutDuration));

        alarmSequence.OnComplete(OnAlarmFinished);
    }

    private void OnAlarmFinished()
    {
        root.alpha = 0f;
        root.blocksRaycasts = false;
        root.interactable = false;
        SetRootActive(false);

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    private void StopAlarm()
    {
        alarmSequence?.Kill();
        alarmSequence = null;

        audioFadeTween?.Kill();
        audioFadeTween = null;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.volume = 0f;
        }

        if (root != null)
        {
            root.alpha = 0f;
            root.blocksRaycasts = false;
            root.interactable = false;
            SetRootActive(false);
        }
    }

    private void SetRootActive(bool isActive)
    {
        if (root == null)
        {
            return;
        }

        GameObject rootObject = root.gameObject;

        // Avoid disabling the script host object; it would stop event subscriptions.
        if (rootObject == gameObject && !isActive)
        {
            return;
        }

        if (rootObject.activeSelf != isActive)
        {
            rootObject.SetActive(isActive);
        }
    }
}
