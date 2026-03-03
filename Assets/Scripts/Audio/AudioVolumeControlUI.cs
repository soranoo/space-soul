using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Binds a UI slider to an exposed AudioMixer volume parameter (in dB)
/// and shows the current value as a percentage text.
/// </summary>
public class AudioVolumeControlUI : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string exposedParameter = "MasterVolume";

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TMP_Text valueText;

    [Header("Range")]
    [SerializeField] private float minDb = -80f;
    [SerializeField] private float maxDb = 0f;

    [Header("Value Display")]
    [SerializeField] private string valueFormat = "{0:0}%";

    private string playerPrefsKey = "";

    private void Awake()
    {
        if (volumeSlider == null)
        {
            volumeSlider = GetComponentInChildren<Slider>(true);
        }

        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.wholeNumbers = false;
        }

        playerPrefsKey = getPlayerfrefKey();
    }

    private void OnEnable()
    {
        if (volumeSlider == null)
        {
            return;
        }

        float normalized = ReadInitialNormalizedValue();
        volumeSlider.SetValueWithoutNotify(normalized);
        SetMixerVolume(NormalizedToDb(normalized));
        UpdateValueText(normalized);

        volumeSlider.onValueChanged.AddListener(HandleSliderChanged);
    }

    private void OnDisable()
    {
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(HandleSliderChanged);
        }
    }

    private void HandleSliderChanged(float normalizedValue)
    {
        float decibelValue = NormalizedToDb(normalizedValue);
        SetMixerVolume(decibelValue);
        SaveNormalizedValue(normalizedValue);
        UpdateValueText(normalizedValue);
    }

    private float ReadInitialNormalizedValue()
    {
        if (!string.IsNullOrWhiteSpace(playerPrefsKey) && PlayerPrefs.HasKey(playerPrefsKey))
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(playerPrefsKey));
        }

        return ReadMixerAsNormalized();
    }

    private void SaveNormalizedValue(float normalizedValue)
    {
        if (string.IsNullOrWhiteSpace(playerPrefsKey))
        {
            return;
        }

        PlayerPrefs.SetFloat(playerPrefsKey, Mathf.Clamp01(normalizedValue));
        PlayerPrefs.Save();
    }

    private float ReadMixerAsNormalized()
    {
        if (mixer == null || string.IsNullOrWhiteSpace(exposedParameter))
        {
            return 1f;
        }

        if (!mixer.GetFloat(exposedParameter, out float dbValue))
        {
            return 1f;
        }

        return DbToNormalized(dbValue);
    }

    private void SetMixerVolume(float dbValue)
    {
        if (mixer == null || string.IsNullOrWhiteSpace(exposedParameter))
        {
            return;
        }

        mixer.SetFloat(exposedParameter, dbValue);
    }

    private float NormalizedToDb(float normalized)
    {
        float clamped = Mathf.Clamp01(normalized);
        return Mathf.Lerp(minDb, maxDb, clamped);
    }

    private float DbToNormalized(float db)
    {
        float range = maxDb - minDb;
        if (Mathf.Approximately(range, 0f))
        {
            return 1f;
        }

        return Mathf.Clamp01((db - minDb) / range);
    }

    private void UpdateValueText(float normalized)
    {
        float percent = Mathf.Clamp01(normalized) * 100f;
        string formatted = string.Format(valueFormat, percent);

        if (valueText != null)
        {
            valueText.text = formatted;
        }
    }

    private string getPlayerfrefKey()
    {
        return "Audio.Volume." + exposedParameter;
    }
}
