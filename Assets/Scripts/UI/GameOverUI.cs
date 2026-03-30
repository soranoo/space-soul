using UnityEngine;

/// <summary>
/// Handles game over UI visibility and button actions.
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private SceneId gameplayScene = SceneId.Gameplay;
    [SerializeField] private SceneId mainMenuScene = SceneId.MainMenu;

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

        if (gameplayScene == SceneId.None)
        {
            Debug.LogError("Gameplay scene is not set on GameOverUI.");
            return;
        }

        Time.timeScale = 1f;
        SceneTransitionManager.Instance.TryTransitionTo(gameplayScene, () =>
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

        if (mainMenuScene == SceneId.None)
        {
            Debug.LogError("Main menu scene is not set on GameOverUI.");
            return;
        }

        GameManager.Instance.ReturnToMainMenu();
        Time.timeScale = 1f;
        SceneTransitionManager.Instance.TryTransitionTo(mainMenuScene);
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
