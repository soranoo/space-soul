/// <summary>
/// Finite state machine that owns and updates the active game state.
/// </summary>
public class GameStateMachine
{
    private IGameState _currentState;

    /// <summary>
    /// Currently active state.
    /// </summary>
    public IGameState CurrentState => _currentState;

    /// <summary>
    /// Transition to a new state.
    /// </summary>
    public void ChangeState(IGameState newState)
    {
        if (newState == null || newState == _currentState)
        {
            return;
        }

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    /// <summary>
    /// Update the active state each frame.
    /// </summary>
    public void Update()
    {
        _currentState?.Update();
    }

    /// <summary>
    /// Let the active state process input.
    /// </summary>
    public void HandleInput()
    {
        _currentState?.HandleInput();
    }
}
