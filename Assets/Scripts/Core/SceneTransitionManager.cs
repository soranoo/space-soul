using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Centralized scene transition entry point with optional delay and post-load callback.
/// </summary>
public class SceneTransitionManager : SingletonBase<SceneTransitionManager>
{
    [Header("Transition")]
    [SerializeField, Min(0f)] private float fadeDurationSeconds = 0.35f;
    [SerializeField] private Color transitionColor = Color.black;
    [SerializeField] private int transitionCanvasSortOrder = 10_000;

    private bool isTransitioning;
    private string pendingSceneName;
    private Action pendingOnLoaded;
    private bool loadedPendingScene;

    private Canvas transitionCanvas;
    private CanvasGroup transitionCanvasGroup;
    private Image transitionImage;

    /// <summary>
    /// Whether a transition is currently in progress.
    /// </summary>
    public bool IsTransitioning => isTransitioning;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    /// <summary>
    /// Starts a scene transition.
    /// </summary>
    public bool TryTransitionTo(SceneId sceneId, Action onLoaded = null)
    {
        string sceneName = sceneId.AsString();
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"Cannot transition scene because SceneId '{sceneId}' is not mapped.");
            return false;
        }

        return TryTransitionTo(sceneName, onLoaded);
    }

    /// <summary>
    /// Starts a scene transition by scene name.
    /// </summary>
    public bool TryTransitionTo(string sceneName, Action onLoaded = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Cannot transition scene because scene name is empty.");
            return false;
        }

        if (isTransitioning)
        {
            Debug.LogWarning($"Scene transition already in progress to '{pendingSceneName}'.");
            return false;
        }

        EnsureTransitionOverlay();
        isTransitioning = true;
        loadedPendingScene = false;
        pendingSceneName = sceneName;
        pendingOnLoaded = onLoaded;
        StartCoroutine(TransitionRoutine());
        return true;
    }

    private IEnumerator TransitionRoutine()
    {
        transitionCanvasGroup.blocksRaycasts = true;
        yield return FadeCanvasTo(1f);

        SceneManager.LoadScene(pendingSceneName);
        yield return new WaitUntil(() => loadedPendingScene);

        Action onLoaded = pendingOnLoaded;
        pendingOnLoaded = null;
        onLoaded?.Invoke();

        yield return FadeCanvasTo(0f);
        transitionCanvasGroup.blocksRaycasts = false;

        isTransitioning = false;
        pendingSceneName = null;
        loadedPendingScene = false;
    }

    private IEnumerator FadeCanvasTo(float targetAlpha)
    {
        if (fadeDurationSeconds <= 0f)
        {
            transitionCanvasGroup.alpha = targetAlpha;
            yield break;
        }

        float startAlpha = transitionCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDurationSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDurationSeconds);
            transitionCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        transitionCanvasGroup.alpha = targetAlpha;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isTransitioning || scene.name != pendingSceneName)
        {
            return;
        }

        loadedPendingScene = true;
    }

    private void EnsureTransitionOverlay()
    {
        if (transitionCanvas != null && transitionCanvasGroup != null && transitionImage != null)
        {
            transitionImage.color = transitionColor;
            transitionCanvas.sortingOrder = transitionCanvasSortOrder;
            return;
        }

        GameObject canvasObject = new GameObject("SceneTransitionCanvas");
        canvasObject.transform.SetParent(transform, false);

        transitionCanvas = canvasObject.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = transitionCanvasSortOrder;

        canvasObject.AddComponent<GraphicRaycaster>();

        transitionCanvasGroup = canvasObject.AddComponent<CanvasGroup>();
        transitionCanvasGroup.alpha = 0f;
        transitionCanvasGroup.blocksRaycasts = false;
        transitionCanvasGroup.interactable = false;

        GameObject imageObject = new GameObject("Fade");
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.AddComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        transitionImage = imageObject.AddComponent<Image>();
        transitionImage.color = transitionColor;
        transitionImage.raycastTarget = true;
    }
}