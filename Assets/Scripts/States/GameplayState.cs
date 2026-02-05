using UnityEngine;

/// <summary>
/// Main game loop and combat state.
/// </summary>
public class GameplayState : BaseGameState
{
    private WaveManager waveManager;
    private bool isSubscribed;

    public GameplayState(GameManager gameManager) : base(gameManager)
    {
    }

    public override void Enter()
    {
        Time.timeScale = 1f;
        EnsureWaveManager();
        StartWaveIfNeeded();
        base.Enter();
    }

    public override void Exit()
    {
        UnsubscribeFromWaveEvents();
        base.Exit();
    }

    public override void HandleInput()
    {
        if (GameManager.IsCancelPressed())
        {
            GameManager.PauseGame();
        }
    }

    private void EnsureWaveManager()
    {
        if (waveManager == null)
        {
            waveManager = WaveManager.Instance;
            Debug.Log($"[GameplayState] WaveManager.Instance is {(waveManager != null ? "found" : "NULL")}");
        }

        if (waveManager != null && !isSubscribed)
        {
            waveManager.WaveCompleted += OnWaveCompleted;
            isSubscribed = true;
            Debug.Log("[GameplayState] Subscribed to WaveManager.WaveCompleted");
        }
    }

    private void UnsubscribeFromWaveEvents()
    {
        if (waveManager != null && isSubscribed)
        {
            waveManager.WaveCompleted -= OnWaveCompleted;
            isSubscribed = false;
        }
    }

    private void StartWaveIfNeeded()
    {
        Debug.Log($"[GameplayState] StartWaveIfNeeded called. waveManager={waveManager != null}, IsWaveActive={waveManager?.IsWaveActive}, CurrentWaveNumber={waveManager?.CurrentWaveNumber}");

        if (waveManager == null)
        {
            Debug.LogWarning("[GameplayState] WaveManager is null, cannot start waves.");
            return;
        }

        if (waveManager.IsWaveActive)
        {
            Debug.Log("[GameplayState] Wave already active, skipping.");
            return;
        }

        if (waveManager.CurrentWaveNumber <= 0)
        {
            Debug.Log("[GameplayState] Calling WaveManager.StartWaves()");
            waveManager.StartWaves();
        }
        else
        {
            Debug.Log($"[GameplayState] Calling WaveManager.StartNextWave() for wave {waveManager.CurrentWaveNumber + 1}");
            waveManager.StartNextWave();
        }
    }

    private void OnWaveCompleted(int waveNumber)
    {
        GameManager.CompleteWave();
    }
}
