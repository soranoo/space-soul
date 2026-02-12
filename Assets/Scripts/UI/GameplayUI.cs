using DG.Tweening;
using UnityEngine;

/// <summary>
/// Controls gameplay HUD visibility and fades based on game state.
/// Visible in GameplayState, hidden in all other states.
/// </summary>
public class GameplayUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;

    [Header("Fade")]
    [SerializeField] private float fadeDuration = 0.2f;

    private CanvasGroup canvasGroup;
    private GameStateMachine stateMachine;
    private Tween fadeTween;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        if (root != null)
        {
            canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = root.AddComponent<CanvasGroup>();
            }
        }
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        stateMachine = GameManager.Instance.StateMachine;
        stateMachine.StateChanged += HandleStateChanged;
        HandleStateChanged(stateMachine.PreviousState, stateMachine.CurrentState);
    }

    private void OnDisable()
    {
        if (stateMachine != null)
        {
            stateMachine.StateChanged -= HandleStateChanged;
        }

        fadeTween?.Kill();
        fadeTween = null;
    }

    private void HandleStateChanged(IGameState previousState, IGameState newState)
    {
        if (root == null || canvasGroup == null)
        {
            return;
        }

        bool shouldShow = newState is GameplayState;
        StartFade(shouldShow);
    }

    private void StartFade(bool show)
    {
        fadeTween?.Kill();
        fadeTween = null;

        if (show && !root.activeSelf)
        {
            root.SetActive(true);
        }
        float target = show ? 1f : 0f;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        if (fadeDuration <= 0f)
        {
            canvasGroup.alpha = target;

            if (!show)
            {
                root.SetActive(false);
            }

            return;
        }

        fadeTween = canvasGroup
            .DOFade(target, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (!show)
                {
                    root.SetActive(false);
                }

                fadeTween = null;
            });
    }
}
