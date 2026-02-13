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

    [SerializeField] private bool randomizePitch;

    [Range(-3f, 3f)]
    [SerializeField] private float pitchMin = 0.95f;

    [Range(-3f, 3f)]
    [SerializeField] private float pitchMax = 1.05f;

    [SerializeField] private AudioMixerGroup mixerGroup;

    public float Volume => volume;
    public float Pitch => pitch;
    public bool RandomizePitch => randomizePitch;
    public float PitchMin => pitchMin;
    public float PitchMax => pitchMax;
    public AudioMixerGroup MixerGroup => mixerGroup;

    public void ApplyTo(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.volume = volume;

        if (randomizePitch)
        {
            float minPitch = Mathf.Clamp(Mathf.Min(pitchMin, pitchMax), -3f, 3f);
            float maxPitch = Mathf.Clamp(Mathf.Max(pitchMin, pitchMax), -3f, 3f);
            source.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            source.pitch = pitch;
        }

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
