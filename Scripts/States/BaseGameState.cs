using UnityEngine;

/// <summary>
/// Base class for game states with shared helpers.
/// </summary>
public abstract class BaseGameState : IGameState
{
    protected readonly GameManager GameManager;

    protected BaseGameState(GameManager gameManager)
    {
        GameManager = gameManager;
    }

    public virtual void Enter()
    {
        LogStateEnter();
    }

    public virtual void Update()
    {
    }

    public virtual void Exit()
    {
        LogStateExit();
    }

    public virtual void HandleInput()
    {
    }

    protected void LogStateEnter()
    {
        if (GameManager != null && GameManager.LogStateChanges)
        {
            Debug.Log($"Enter {GetType().Name}");
        }
    }

    protected void LogStateExit()
    {
        if (GameManager != null && GameManager.LogStateChanges)
        {
            Debug.Log($"Exit {GetType().Name}");
        }
    }
}
