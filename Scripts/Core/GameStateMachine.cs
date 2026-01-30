/// <summary>
/// Finite state machine that owns and updates the active game state.
/// </summary>
public class GameStateMachine
{
    /// <summary>
    /// Fired after a state transition completes.
    /// Parameters: previous state, current state.
    /// </summary>
    public event System.Action<IGameState, IGameState> StateChanged;

    /// <summary>
    /// Currently active state.
    /// </summary>
    public IGameState CurrentState { get; private set; }

    /// <summary>
    /// Most recent state prior to the current one.
    /// </summary>
    public IGameState PreviousState { get; private set; }

    /// <summary>
    /// Transition to a new state.
    /// </summary>
    public void ChangeState(IGameState newState)
    {
        if (newState == null || newState == CurrentState)
        {
            return;
        }

        PreviousState = CurrentState;
        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();

        StateChanged?.Invoke(PreviousState, CurrentState);
    }

    /// <summary>
    /// Update the active state each frame.
    /// </summary>
    public void Update()
    {
        CurrentState?.Update();
    }

    /// <summary>
    /// Let the active state process input.
    /// </summary>
    public void HandleInput()
    {
        CurrentState?.HandleInput();
    }
}
