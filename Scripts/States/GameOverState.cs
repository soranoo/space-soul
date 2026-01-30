using UnityEngine;

/// <summary>
/// Displays final stats and handles restart flow.
/// </summary>
public class GameOverState : BaseGameState
{
    public GameOverState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 0f;
        base.Enter();
    }

    public override void HandleInput()
    {
        if (GameManager.IsSubmitPressed() || GameManager.IsAttackPressed())
        {
            GameManager.ReturnToMainMenu();
        }
    }
}
