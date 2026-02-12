using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Centralized runtime SFX playback service. Persists across scenes.
/// Callers pass their own AudioSettings (and optionally their own AudioSource).
/// </summary>
public class SfxManager : SingletonBase<SfxManager>
{
    [Header("Output")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Range(0f, 1f)]
    [SerializeField] private float masterSfxVolume = 1f;

    private AudioSource defaultSource;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        defaultSource = GetComponent<AudioSource>();
        if (defaultSource == null)
        {
            defaultSource = gameObject.AddComponent<AudioSource>();
        }

        defaultSource.playOnAwake = false;
        defaultSource.loop = false;

        if (sfxMixerGroup != null)
        {
            defaultSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    /// <summary>
    /// Play an SFX using the given AudioSettings on the built-in source.
    /// </summary>
    public void Play(AudioSettings settings)
    {
        PlayInternal(settings, null);
    }

    /// <summary>
    /// Play an SFX using the given AudioSettings on an external AudioSource.
    /// Falls back to the built-in source when null.
    /// </summary>
    public void Play(AudioSettings settings, AudioSource source)
    {
        PlayInternal(settings, source);
    }

    private void PlayInternal(AudioSettings settings, AudioSource externalSource)
    {
        if (settings == null || settings.Clip == null)
        {
            return;
        }

        AudioSource source = externalSource != null ? externalSource : defaultSource;
        if (source == null)
        {
            return;
        }

        if (settings.Source != null)
        {
            settings.Source.ApplyTo(source);

            if (settings.Source.MixerGroup == null && sfxMixerGroup != null)
            {
                source.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        float configuredVolume = settings.Source != null ? settings.Source.Volume : 1f;
        float finalVolume = Mathf.Clamp01(masterSfxVolume) * Mathf.Clamp01(configuredVolume);
        source.PlayOneShot(settings.Clip, finalVolume);
    }
}
