using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Source-style playback configuration shared by both BGM and SFX.
/// </summary>
[System.Serializable]
public class AudioSourceConfig
{
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Range(-3f, 3f)]
    [SerializeField] private float pitch = 1f;

    [Range(-1f, 1f)]
    [SerializeField] private float stereoPan = 0f;

    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 0f;

    [SerializeField] private bool loop;
    [SerializeField] private bool mute;
    [SerializeField] private AudioMixerGroup mixerGroup;

    public float Volume => volume;
    public float Pitch => pitch;
    public float StereoPan => stereoPan;
    public float SpatialBlend => spatialBlend;
    public bool Loop => loop;
    public bool Mute => mute;
    public AudioMixerGroup MixerGroup => mixerGroup;

    public void ApplyTo(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.volume = volume;
        source.pitch = pitch;
        source.panStereo = stereoPan;
        source.spatialBlend = spatialBlend;
        source.loop = loop;
        source.mute = mute;

        if (mixerGroup != null)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }
    }
}

/// <summary>
/// A single configurable audio item.
/// </summary>
[CreateAssetMenu(fileName = "AudioSettings", menuName = "Game/Audio Settings")]
public class AudioSettings : ScriptableObject
{
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioSourceConfig source = new AudioSourceConfig();

    public AudioClip Clip => clip;
    public AudioSourceConfig Source => source;
}
