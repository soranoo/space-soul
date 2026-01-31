using UnityEngine;

/// <summary>
/// Idle state - enemy waits and looks for player.
/// Transitions to Chase when player is detected.
/// </summary>
public class EnemyIdleState : BaseEnemyState
{
    public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine) 
        : base(enemy, stateMachine) { }

    public override void Update()
    {
        // Check if player is in detection range
        if (enemy.IsPlayerInRange(enemy.Data.DetectionRange))
        {
            stateMachine.ChangeState<EnemyChaseState>();
        }
    }
}
