using UnityEngine;

/// <summary>
/// Brief transition between waves.
/// </summary>
public class WaveCompleteState : BaseGameState
{
    public WaveCompleteState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 0f;
        base.Enter();
    }

    public override void HandleInput()
    {
        // Placeholder: advance to upgrade selection.
        if (GameManager.IsSubmitPressed() || GameManager.IsInteractPressed())
        {
            GameManager.ShowUpgradeSelection();
        }
    }
}
