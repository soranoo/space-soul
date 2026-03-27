using System;
using System.Collections.Generic;

/// <summary>
/// Controls individual enemy behavior states.
/// Implements FSM pattern for enemy AI.
/// </summary>
public class EnemyStateMachine
{
    private IEnemyState currentState;
    private readonly Dictionary<Type, IEnemyState> states;

    /// <summary>
    /// The currently active state.
    /// </summary>
    public IEnemyState CurrentState => currentState;

    /// <summary>
    /// Creates a new enemy state machine.
    /// </summary>
    /// <param name="enemy">Enemy context that states operate on.</param>
    public EnemyStateMachine(Enemy enemy)
    {
        states = new Dictionary<Type, IEnemyState>
        {
            { typeof(EnemyIdleState), new EnemyIdleState(enemy, this) },
            { typeof(EnemyChaseState), new EnemyChaseState(enemy, this) },
            { typeof(EnemyAttackState), new EnemyAttackState(enemy, this) }
        };
    }

    /// <summary>
    /// Transition to a new state.
    /// </summary>
    /// <typeparam name="T">Type of cached state to activate.</typeparam>
    public void ChangeState<T>() where T : IEnemyState
    {
        if (!states.TryGetValue(typeof(T), out IEnemyState newState))
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
