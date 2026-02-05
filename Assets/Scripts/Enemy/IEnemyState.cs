/// <summary>
/// Contract for enemy AI states.
/// </summary>
public interface IEnemyState
{
    /// <summary>
    /// Called when entering this state.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called each frame while this state is active.
    /// </summary>
    void Update();

    /// <summary>
    /// Called when exiting this state.
    /// </summary>
    void Exit();
}
