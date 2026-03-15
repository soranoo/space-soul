using DG.Tweening;
using UnityEngine;

/// <summary>
/// Shows a control tutorial overlay, fades in on start, and dismisses after first control input.
/// </summary>
public class ControlTutorialUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private InputHandler inputHandler;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("Input Detection")]
    [SerializeField] private float moveInputThreshold = 0.01f;

    private Tween fadeTween;
    private bool hasDismissed;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        if (canvasGroup == null && root != null)
        {
            canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = root.AddComponent<CanvasGroup>();
            }
        }

        if (inputHandler == null)
        {
            inputHandler = FindFirstObjectByType<InputHandler>();
        }
    }

    private void OnEnable()
    {
        hasDismissed = false;

        if (root != null && !root.activeSelf)
        {
            root.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (inputHandler != null)
        {
            inputHandler.FireStarted += OnFirstControlInput;
        }

        FadeTo(1f, fadeInDuration, null);
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.FireStarted -= OnFirstControlInput;
        }

        fadeTween?.Kill();
        fadeTween = null;
    }

    private void Update()
    {
        if (hasDismissed || inputHandler == null)
        {
            return;
        }

        if (inputHandler.MoveInput.sqrMagnitude > moveInputThreshold * moveInputThreshold)
        {
            OnFirstControlInput();
        }
    }

    private void OnFirstControlInput()
    {
        if (hasDismissed)
        {
            return;
        }

        hasDismissed = true;

        if (inputHandler != null)
        {
            inputHandler.FireStarted -= OnFirstControlInput;
        }

        FadeTo(0f, fadeOutDuration, () =>
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        });
    }

    private void FadeTo(float targetAlpha, float duration, TweenCallback onComplete)
    {
        fadeTween?.Kill();
        fadeTween = null;

        if (canvasGroup == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
            return;
        }

        fadeTween = canvasGroup
            .DOFade(targetAlpha, duration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                onComplete?.Invoke();
                fadeTween = null;
            });
    }
}
