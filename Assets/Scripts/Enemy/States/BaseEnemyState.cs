/// <summary>
/// Base class for enemy AI states.
/// Provides common functionality and references.
/// </summary>
public abstract class BaseEnemyState : IEnemyState
{
    protected Enemy enemy;
    protected EnemyStateMachine stateMachine;

    /// <summary>
    /// Creates a new enemy state.
    /// </summary>
    /// <param name="enemy">The enemy this state controls.</param>
    /// <param name="stateMachine">The state machine managing this state.</param>
    public BaseEnemyState(Enemy enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    /// <summary>
    /// Called when entering this state.
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// Called each frame while this state is active.
    /// </summary>
    public abstract void Update();

    /// <summary>
    /// Called when exiting this state.
    /// </summary>
    public virtual void Exit() { }
}
