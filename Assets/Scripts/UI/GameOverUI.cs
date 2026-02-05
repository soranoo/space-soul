using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is not set on GameOverUI.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Return to main menu scene.
    /// </summary>
    public void OnMainMenuPressed()
    {
        GameManager.Instance.ReturnToMainMenu();

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("Main menu scene name is not set on GameOverUI.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
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
