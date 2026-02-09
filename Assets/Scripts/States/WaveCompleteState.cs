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

        // Immediately show upgrades when a wave completes.
        GameManager.ShowUpgradeSelection();
    }

    public override void HandleInput()
    {
        // Input no longer needed; upgrade selection is shown on Enter.
    }
}
