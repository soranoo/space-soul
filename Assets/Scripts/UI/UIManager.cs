using UnityEngine;

/// <summary>
/// Centralized UI-level behaviors such as button click SFX.
/// </summary>
public class UIManager : SingletonBase<UIManager>
{
    [Header("Button SFX")]
    [SerializeField] private AudioSettings onButtonClickSfx;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Override the button click SFX at runtime.
    /// </summary>
    public void SetButtonClickSfx(AudioSettings settings)
    {
        onButtonClickSfx = settings;
    }

    /// <summary>
    /// Plays the configured button click SFX.
    /// </summary>
    public void OnButtonClick()
    {
        PlayButtonClickSfx(onButtonClickSfx);
    }

    /// <summary>
    /// Plays the provided button click SFX.
    /// </summary>
    public void PlayButtonClickSfx(AudioSettings settings)
    {
        if (settings == null)
        {
            return;
        }

        SfxManager.Instance?.Play(settings);
    }
}