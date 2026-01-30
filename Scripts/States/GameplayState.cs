using UnityEngine;

/// <summary>
/// Main game loop and combat state.
/// </summary>
public class GameplayState : BaseGameState
{
    public GameplayState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 1f;
        base.Enter();
    }

    public override void HandleInput()
    {
        if (GameManager.IsCancelPressed())
        {
            GameManager.PauseGame();
        }
    }
}
