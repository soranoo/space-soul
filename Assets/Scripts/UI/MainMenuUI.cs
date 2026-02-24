using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles main menu button actions.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Credits")]
    [SerializeField] private CreditsPanelUI creditsPanel;

    [Header("Audio")]
    [SerializeField] private AudioSettings mainMenuBgmSettings;

    private bool waitingForGameplaySceneLoad;

    private void OnEnable()
    {
        if (mainMenuBgmSettings != null)
        {
            BgmManager.Instance?.SetTrackImmediate(mainMenuBgmSettings);
        }

        creditsPanel?.Hide();
    }

    private void OnDisable()
    {
        if (waitingForGameplaySceneLoad)
        {
            return;
        }

        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
    }

    /// <summary>
    /// Loads the gameplay scene when Start is pressed.
    /// </summary>
    public void OnStartGamePressed()
    {
        UIManager.Instance.OnButtonClick();
        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError("Gameplay scene name is not set on MainMenuUI.");
            return;
        }

        Time.timeScale = 1f;

        if (SceneManager.GetActiveScene().name == gameplaySceneName)
        {
            GameManager.Instance?.StartGame();
            return;
        }

        waitingForGameplaySceneLoad = true;
        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
        SceneManager.sceneLoaded += OnGameplaySceneLoaded;
        SceneManager.LoadScene(gameplaySceneName);
    }

    private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != gameplaySceneName)
        {
            return;
        }

        waitingForGameplaySceneLoad = false;
        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
        GameManager.Instance?.StartGame();
    }

    /// <summary>
    /// Shows the credits panel.
    /// </summary>
    public void OnCreditsPressed()
    {
        UIManager.Instance.OnButtonClick();
        creditsPanel?.Show();
    }

    /// <summary>
    /// Closes the credits panel.
    /// </summary>
    public void OnCloseCreditsPressed()
    {
        UIManager.Instance.OnButtonClick();
        creditsPanel?.Hide();
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void OnQuitPressed()
    {
        UIManager.Instance.OnButtonClick();
        Application.Quit();
    }
}
