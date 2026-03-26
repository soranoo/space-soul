using UnityEngine;

/// <summary>
/// Idle state - enemy waits and looks for player.
/// Transitions to Chase whenever a player reference exists.
/// </summary>
public class EnemyIdleState : BaseEnemyState
{
    public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine) 
        : base(enemy, stateMachine) { }

    public override void Update()
    {
        if (enemy.TryFindPlayerReference())
        {
            stateMachine.ChangeState(new EnemyChaseState(enemy, stateMachine));
        }
    }
}
