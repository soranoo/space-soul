using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles pause menu UI and button actions.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
	[Header("Scene Loading")]
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
		stateMachine = GameManager.Instance.StateMachine;
		stateMachine.StateChanged += HandleStateChanged;
		HandleStateChanged(stateMachine.PreviousState, stateMachine.CurrentState);
	}

	private void OnDisable()
	{
		stateMachine.StateChanged -= HandleStateChanged;
	}

	/// <summary>
	/// Show the pause menu.
	/// </summary>
	public void Show()
	{
		root?.SetActive(true);
	}

	/// <summary>
	/// Hide the pause menu.
	/// </summary>
	public void Hide()
	{
		root?.SetActive(false);
	}

	/// <summary>
	/// Resume gameplay.
	/// </summary>
	public void OnResumePressed()
	{
		GameManager.Instance.ResumeGame();
	}

	/// <summary>
	/// Return to main menu scene.
	/// </summary>
	public void OnMainMenuPressed()
	{
		GameManager.Instance.ReturnToMainMenu();

		if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
		{
			Time.timeScale = 1f;
			SceneManager.LoadScene(mainMenuSceneName);
		}
	}

	private void HandleStateChanged(IGameState previousState, IGameState newState)
	{
		if (root == null)
		{
			return;
		}

		var shouldShow = newState is PausedState;
		root.SetActive(shouldShow);
	}
}
