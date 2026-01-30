public interface IGameState
{
    /// <summary>
    /// Called when the state becomes active.
    /// </summary>
    void Enter();

    /// <summary>
    /// Called once per frame while this state is active.
    /// </summary>
    void Update();

    /// <summary>
    /// Called when leaving this state.
    /// </summary>
    void Exit();

    /// <summary>
    /// Called to process input while the state is active.
    /// </summary>
    void HandleInput();
}
