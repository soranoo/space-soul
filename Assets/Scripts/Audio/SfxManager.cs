using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Centralized runtime SFX playback for gameplay events.
/// </summary>
public class SfxManager : SingletonBase<SfxManager>
{
    [Header("SFX Settings")]
    [SerializeField] private AudioSettings waveCompletedSettings;
    [SerializeField] private AudioSettings powerUpCollectedSettings;
    [SerializeField] private AudioSettings enemySelfDestructSettings;

    [Header("Output")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [Range(0f, 1f)]
    [SerializeField] private float masterSfxVolume = 1f;

    private AudioSource sfxSource;

    protected override void Awake()
    {
        base.Awake();

        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        if (sfxMixerGroup != null)
        {
            sfxSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    public void PlayWaveCompleted()
    {
        PlayFromSettings(waveCompletedSettings);
    }

    public void PlayPowerUpCollected()
    {
        PlayFromSettings(powerUpCollectedSettings);
    }

    public void PlayEnemySelfDestruct()
    {
        PlayFromSettings(enemySelfDestructSettings);
    }

    private void PlayFromSettings(AudioSettings settings)
    {
        if (settings == null || sfxSource == null || settings.Clip == null)
        {
            return;
        }

        if (settings.Source != null)
        {
            settings.Source.ApplyTo(sfxSource);

            if (settings.Source.MixerGroup == null && sfxMixerGroup != null)
            {
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            }
        }

        float configuredVolume = settings.Source != null ? settings.Source.Volume : 1f;
        float finalVolume = Mathf.Clamp01(masterSfxVolume) * Mathf.Clamp01(configuredVolume);
        sfxSource.PlayOneShot(settings.Clip, finalVolume);
    }
}
