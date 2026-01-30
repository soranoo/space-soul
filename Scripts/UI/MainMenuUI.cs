using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles main menu button actions.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "Gameplay";

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
        SceneManager.LoadScene(gameplaySceneName);
    }
}
