using TMPro;
using UnityEngine;

/// <summary>
/// Displays an FPS counter and supports runtime visibility toggling.
/// </summary>
public class FpsCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text fpsText;

    [Header("Display")]
    [SerializeField] private string format = "FPS: {0:0}";

    [Range(0.05f, 1f)]
    [SerializeField] private float refreshInterval = 0.25f;

    [SerializeField] private bool visibleOnStart = false;

    private const string ShowFpsPrefsKey = "UI.ShowFpsCounter";

    private float nextRefreshTime;
    private float smoothedDeltaTime;

    /// <summary>
    /// Returns true when the counter root GameObject is active.
    /// </summary>
    public bool IsVisible => root != null && root.activeSelf;

    private void Awake()
    {
        if (fpsText == null)
        {
            fpsText = GetComponentInChildren<TMP_Text>(true);
        }

        if (root == null)
        {
            root = fpsText != null ? fpsText.gameObject : gameObject;
        }

        smoothedDeltaTime = Time.unscaledDeltaTime;

        bool shouldShow = PlayerPrefs.GetInt(ShowFpsPrefsKey, visibleOnStart ? 1 : 0) == 1;
        SetVisible(shouldShow);
    }

    private void Update()
    {
        if (root == null || !root.activeInHierarchy || fpsText == null)
        {
            return;
        }

        smoothedDeltaTime = Mathf.Lerp(smoothedDeltaTime, Time.unscaledDeltaTime, 0.1f);

        if (Time.unscaledTime < nextRefreshTime)
        {
            return;
        }

        nextRefreshTime = Time.unscaledTime + refreshInterval;

        float fps = smoothedDeltaTime > 0f ? (1f / smoothedDeltaTime) : 0f;
        fpsText.text = string.Format(format, fps);
    }

    /// <summary>
    /// Shows or hides the FPS counter and persists the preference.
    /// </summary>
    public void SetVisible(bool isVisible)
    {
        if (root != null)
        {
            root.SetActive(isVisible);
        }

        PlayerPrefs.SetInt(ShowFpsPrefsKey, isVisible ? 1 : 0);
        PlayerPrefs.Save();
    }
}