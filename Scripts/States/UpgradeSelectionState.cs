using UnityEngine;

/// <summary>
/// Presents upgrade choices and applies the selection.
/// </summary>
public class UpgradeSelectionState : BaseGameState
{
    public UpgradeSelectionState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 0f;
        base.Enter();
    }

    public override void HandleInput()
    {
        // Placeholder input: choose any upgrade using mapped actions.
        if (GameManager.IsPreviousPressed() ||
            GameManager.IsNextPressed() ||
            GameManager.IsInteractPressed() ||
            GameManager.IsSubmitPressed() ||
            GameManager.IsAttackPressed())
        {
            GameManager.StartNextWave();
        }
    }
}
