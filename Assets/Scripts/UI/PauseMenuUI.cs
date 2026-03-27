using UnityEngine;

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
		UIManager.Instance.OnButtonClick();
		GameManager.Instance.ResumeGame();
	}

	/// <summary>
	/// Return to main menu scene.
	/// </summary>
	public void OnMainMenuPressed()
	{
		UIManager.Instance.OnButtonClick();

		if (string.IsNullOrWhiteSpace(mainMenuSceneName))
		{
			Debug.LogError("Main menu scene name is not set on PauseMenuUI.");
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

		bool shouldShow = newState is PausedState;
		root.SetActive(shouldShow);
	}
}
