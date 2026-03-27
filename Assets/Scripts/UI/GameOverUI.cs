using UnityEngine;

/// <summary>
/// Handles game over UI visibility and button actions.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "Gameplay";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private GameObject root;

    private GameStateMachine stateMachine;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
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
    }

    /// <summary>
    /// Restart the gameplay scene.
    /// </summary>
    public void OnRestartPressed()
    {
        UIManager.Instance.OnButtonClick();

        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is not set on GameOverUI.");
            return;
        }

        Time.timeScale = 1f;
        SceneTransitionManager.Instance.TryTransitionTo(gameplaySceneName, () =>
        {
            GameManager.Instance?.StartGame();
        });
    }

    /// <summary>
    /// Return to main menu scene.
    /// </summary>
    public void OnMainMenuPressed()
    {
        UIManager.Instance.OnButtonClick();

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("Main menu scene name is not set on GameOverUI.");
            return;
        }

        GameManager.Instance.ReturnToMainMenu();
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.TryTransitionTo(mainMenuSceneName);
    }

    private void HandleStateChanged(IGameState previousState, IGameState newState)
    {
        if (root == null)
        {
            return;
        }

        bool shouldShow = newState is GameOverState;
        root.SetActive(shouldShow);
    }
}
