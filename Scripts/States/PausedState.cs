using UnityEngine;

/// <summary>
/// Pauses gameplay and shows pause menu.
/// </summary>
public class PausedState : BaseGameState
{
    public PausedState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 0f;
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
        Time.timeScale = 1f;
    }

    public override void HandleInput()
    {
        if (GameManager.IsCancelPressed())
        {
            GameManager.ResumeGame();
        }
    }
}
