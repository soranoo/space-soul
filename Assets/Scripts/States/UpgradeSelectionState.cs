using UnityEngine;

/// <summary>
/// Presents upgrade choices and applies the selection.
/// </summary>
public class UpgradeSelectionState : BaseGameState
{
    private UpgradeSelectionUI upgradeUI;

    public UpgradeSelectionState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 0f;
        base.Enter();

        upgradeUI = Object.FindAnyObjectByType<UpgradeSelectionUI>();

        if (upgradeUI != null)
        {
            upgradeUI.UpgradeSelected += OnUpgradeSelected;
            upgradeUI.Show();
        }
        else
        {
            // No UI found — just proceed
            GameManager.StartNextWave();
        }
    }

    public override void Exit()
    {
        if (upgradeUI != null)
        {
            upgradeUI.UpgradeSelected -= OnUpgradeSelected;
            upgradeUI.Hide();
        }

        base.Exit();
    }

    public override void HandleInput()
    {
        // Input is handled by UI buttons on the upgrade cards.
    }

    private void OnUpgradeSelected()
    {
        GameManager.StartNextWave();
    }
}
