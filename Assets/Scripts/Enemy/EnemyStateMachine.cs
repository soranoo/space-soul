/// <summary>
/// Controls individual enemy behavior states.
/// Implements FSM pattern for enemy AI.
/// </summary>
public class EnemyStateMachine
{
    private IEnemyState currentState;

    /// <summary>
    /// The currently active state.
    /// </summary>
    public IEnemyState CurrentState => currentState;

    /// <summary>
    /// Creates a new enemy state machine.
    /// </summary>
    public EnemyStateMachine()
    {
    }

    /// <summary>
    /// Transition to a new state.
    /// </summary>
    /// <param name="newState">New state instance to activate.</param>
    public void ChangeState(IEnemyState newState)
    {
        if (newState == null)
        {
            return;
        }

        if (currentState != null)
        {
            currentState.Exit();
        }

        currentState = newState;
        currentState.Enter();
    }

    /// <summary>
    /// Update the current state.
    /// </summary>
    public void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
        }
    }

    /// <summary>
    /// Reset the state machine.
    /// </summary>
    public void Reset()
    {
        if (currentState != null)
        {
            currentState.Exit();
            currentState = null;
        }
    }
}
