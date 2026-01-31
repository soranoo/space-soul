using System.Collections.Generic;

/// <summary>
/// Controls individual enemy behavior states.
/// Implements FSM pattern for enemy AI.
/// </summary>
public class EnemyStateMachine
{
    private IEnemyState currentState;
    private Dictionary<System.Type, IEnemyState> states;

    /// <summary>
    /// The currently active state.
    /// </summary>
    public IEnemyState CurrentState => currentState;

    /// <summary>
    /// Creates a new enemy state machine.
    /// </summary>
    public EnemyStateMachine()
    {
        states = new Dictionary<System.Type, IEnemyState>();
    }

    /// <summary>
    /// Register a state with the state machine.
    /// </summary>
    /// <typeparam name="T">Type of state to register.</typeparam>
    /// <param name="state">State instance.</param>
    public void RegisterState<T>(T state) where T : IEnemyState
    {
        System.Type stateType = typeof(T);
        if (!states.ContainsKey(stateType))
        {
            states[stateType] = state;
        }
    }

    /// <summary>
    /// Get a registered state by type.
    /// </summary>
    /// <typeparam name="T">Type of state to get.</typeparam>
    /// <returns>The state instance, or null if not found.</returns>
    public T GetState<T>() where T : class, IEnemyState
    {
        System.Type stateType = typeof(T);
        if (states.TryGetValue(stateType, out IEnemyState state))
        {
            return state as T;
        }
        return null;
    }

    /// <summary>
    /// Transition to a new state.
    /// </summary>
    /// <typeparam name="T">Type of state to transition to.</typeparam>
    public void ChangeState<T>() where T : IEnemyState
    {
        System.Type stateType = typeof(T);
        if (!states.TryGetValue(stateType, out IEnemyState newState))
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
