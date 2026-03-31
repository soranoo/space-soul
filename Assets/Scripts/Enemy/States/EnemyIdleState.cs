using UnityEngine;

/// <summary>
/// Idle state - enemy waits and looks for player.
/// </summary>
public class EnemyIdleState : BaseEnemyState
{
    public EnemyIdleState(Enemy enemy, EnemyStateMachine stateMachine) 
        : base(enemy, stateMachine) { }

    public override void Update()
    {
        if (enemy.TryFindPlayerReference())
        {
            stateMachine.ChangeState<EnemyChaseState>();
        }
    }
}
