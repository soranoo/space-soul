using UnityEngine;

/// <summary>
/// Handles main menu button actions.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private SceneId gameplayScene = SceneId.Gameplay;

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
        if (gameplayScene == SceneId.None)
        {
            Debug.LogError("Gameplay scene is not set on MainMenuUI.");
            return;
        }

        Time.timeScale = 1f;

        SceneTransitionManager.Instance.TryTransitionTo(gameplayScene, () =>
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
