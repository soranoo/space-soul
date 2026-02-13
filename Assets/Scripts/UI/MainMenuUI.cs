using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles main menu button actions.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Audio")]
    [SerializeField] private AudioSettings mainMenuBgmSettings;

    private bool waitingForGameplaySceneLoad;

    private void OnEnable()
    {
        if (mainMenuBgmSettings != null)
        {
            BgmManager.Instance?.SetTrackImmediate(mainMenuBgmSettings);
        }
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
}
