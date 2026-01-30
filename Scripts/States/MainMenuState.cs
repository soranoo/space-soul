using UnityEngine;

/// <summary>
/// Handles main menu UI and navigation.
/// </summary>
public class MainMenuState : BaseGameState
{
    public MainMenuState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 1f;
        base.Enter();
    }

    public override void HandleInput()
    {
        // Placeholder input for prototype testing.
        if (GameManager.IsSubmitPressed() || GameManager.IsAttackPressed())
        {
            GameManager.StartGame();
        }
    }
}
