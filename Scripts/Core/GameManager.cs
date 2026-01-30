using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central coordinator for game-wide systems and state transitions.
/// </summary>
public class GameManager : SingletonBase<GameManager>
{
    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;

    private GameStateMachine stateMachine;
    private InputSystem_Actions inputActions;

    private MainMenuState mainMenuState;
    private GameplayState gameplayState;
    private WaveCompleteState waveCompleteState;
    private UpgradeSelectionState upgradeSelectionState;
    private PausedState pausedState;
    private GameOverState gameOverState;

    /// <summary>
    /// Exposes the state machine to game states and other systems.
    /// </summary>
    public GameStateMachine StateMachine => stateMachine;

    /// <summary>
    /// Exposes the input actions asset for systems that need direct access.
    /// </summary>
    public InputSystem_Actions InputActions => inputActions;

    /// <summary>
    /// Whether state changes should be logged to the console.
    /// </summary>
    public bool LogStateChanges => logStateChanges;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        inputActions = new InputSystem_Actions();
        stateMachine = new GameStateMachine();

        mainMenuState = new MainMenuState(this);
        gameplayState = new GameplayState(this);
        waveCompleteState = new WaveCompleteState(this);
        upgradeSelectionState = new UpgradeSelectionState(this);
        pausedState = new PausedState(this);
        gameOverState = new GameOverState(this);
    }

    private void Start()
    {
        stateMachine.ChangeState(mainMenuState);
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new InputSystem_Actions();
        }

        inputActions.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }

    private void Update()
    {
        stateMachine.HandleInput();
        stateMachine.Update();
    }

    /// <summary>
    /// Returns true when the UI submit action is pressed this frame.
    /// </summary>
    public bool IsSubmitPressed()
    {
        return inputActions.UI.Submit.WasPressedThisFrame();
    }

    /// <summary>
    /// Returns true when the UI cancel action is pressed this frame.
    /// </summary>
    public bool IsCancelPressed()
    {
        return inputActions.UI.Cancel.WasPressedThisFrame();
    }

    /// <summary>
    /// Returns true when the player attack action is pressed this frame.
    /// </summary>
    public bool IsAttackPressed()
    {
        return inputActions.Player.Attack.WasPressedThisFrame();
    }

    /// <summary>
    /// Returns true when the player interact action is pressed this frame.
    /// </summary>
    public bool IsInteractPressed()
    {
        return inputActions.Player.Interact.WasPressedThisFrame();
    }

    /// <summary>
    /// Returns true when the player next action is pressed this frame.
    /// </summary>
    public bool IsNextPressed()
    {
        return inputActions.Player.Next.WasPressedThisFrame();
    }

    /// <summary>
    /// Returns true when the player previous action is pressed this frame.
    /// </summary>
    public bool IsPreviousPressed()
    {
        return inputActions.Player.Previous.WasPressedThisFrame();
    }

    /// <summary>
    /// Transition into gameplay.
    /// </summary>
    public void StartGame()
    {
        stateMachine.ChangeState(gameplayState);
    }

    /// <summary>
    /// Transition into the wave complete state.
    /// </summary>
    public void CompleteWave()
    {
        stateMachine.ChangeState(waveCompleteState);
    }

    /// <summary>
    /// Transition into the upgrade selection state.
    /// </summary>
    public void ShowUpgradeSelection()
    {
        stateMachine.ChangeState(upgradeSelectionState);
    }

    /// <summary>
    /// Transition back into gameplay after upgrades.
    /// </summary>
    public void StartNextWave()
    {
        stateMachine.ChangeState(gameplayState);
    }

    /// <summary>
    /// Pause the game.
    /// </summary>
    public void PauseGame()
    {
        stateMachine.ChangeState(pausedState);
    }

    /// <summary>
    /// Resume the game from pause.
    /// </summary>
    public void ResumeGame()
    {
        stateMachine.ChangeState(gameplayState);
    }

    /// <summary>
    /// Transition to the game over screen.
    /// </summary>
    public void GameOver()
    {
        stateMachine.ChangeState(gameOverState);
    }

    /// <summary>
    /// Return to the main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        stateMachine.ChangeState(mainMenuState);
    }
}
