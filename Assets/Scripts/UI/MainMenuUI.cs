using UnityEngine;

/// <summary>
/// Handles main menu button actions.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

    [Header("Audio")]
    [SerializeField] private AudioSettings mainMenuBgmSettings;

    private void OnEnable()
    {
        if (mainMenuBgmSettings != null)
        {
            BgmManager.Instance?.SetTrackImmediate(mainMenuBgmSettings);
        }
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

        SceneTransitionManager.Instance.TryTransitionTo(gameplaySceneName, () =>
        {
            GameManager.Instance?.StartGame();
        });
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
