using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a single power-up icon, name, and remaining time.
/// </summary>
public class PowerUpDisplayItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text timeText;

    private float remaining;

    public void Initialize(PowerUpData data, float duration)
    {
        ApplyData(data);
        SetRemaining(duration);
    }

    public void Refresh(PowerUpData data, float duration)
    {
        ApplyData(data);
        SetRemaining(duration);
    }

    private void Update()
    {
        if (remaining <= 0f)
        {
            return;
        }

        remaining -= Time.deltaTime;
        if (remaining < 0f)
        {
            remaining = 0f;
        }

        UpdateTimeText();
    }

    private void ApplyData(PowerUpData data)
    {
        if (data == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.Icon;
            iconImage.enabled = data.Icon != null;
        }

        if (nameText != null)
        {
            string label = string.IsNullOrWhiteSpace(data.DisplayName) ? data.PowerUpType.ToString() : data.DisplayName;
            nameText.text = label;
        }
    }

    private void SetRemaining(float duration)
    {
        remaining = Mathf.Max(0f, duration);
        UpdateTimeText();
    }

    private void UpdateTimeText()
    {
        if (timeText == null)
        {
            return;
        }

        int seconds = Mathf.CeilToInt(remaining);
        timeText.text = seconds > 0 ? $"{seconds}s" : "0s";
    }
}
