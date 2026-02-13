using UnityEngine;

/// <summary>
/// Controls one engine prefab's powering animation and looped engine SFX.
/// Attach to each player engine prefab instance.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class PlayerEngine : MonoBehaviour
{
    private const string ANIM_BOOL_POWERING = "Powering";

    [Header("References")]
    [SerializeField] private Animator engineAnimator;

    [Header("Audio")]
    [SerializeField] private AudioSettings engineSfx;

    private AudioSource audioSource;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void Configure(AudioSettings settings)
    {
        engineSfx = settings;
        EnsureInitialized();
    }

    public void SetPowering(bool isPowering)
    {
        EnsureInitialized();

        if (engineAnimator != null)
        {
            engineAnimator.SetBool(ANIM_BOOL_POWERING, isPowering);
        }

        UpdateEngineAudio(isPowering);
    }

    private void OnDisable()
    {
        SetPowering(false);
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;

        if (engineSfx != null)
        {
            audioSource.clip = engineSfx.Clip;
            engineSfx.Source?.ApplyTo(audioSource);
            audioSource.loop = true;
        }
    }

    private void EnsureInitialized()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (engineAnimator == null)
        {
            engineAnimator = GetComponentInChildren<Animator>();
        }

        ConfigureAudioSource();
    }

    private void UpdateEngineAudio(bool isPowering)
    {
        if (audioSource == null)
        {
            return;
        }

        if (engineSfx == null || engineSfx.Clip == null)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            return;
        }

        if (isPowering)
        {
            if (audioSource.clip != engineSfx.Clip)
            {
                audioSource.clip = engineSfx.Clip;
            }

            if (!audioSource.isPlaying)
            {
                engineSfx.Source?.ApplyTo(audioSource);
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
