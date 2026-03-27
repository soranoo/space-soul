using UnityEngine;

/// <summary>
/// Chase state - enemy pursues the player.
/// Transitions to Attack based on configured behavior.
/// </summary>
public class EnemyChaseState : BaseEnemyState
{
    public EnemyChaseState(Enemy enemy, EnemyStateMachine stateMachine) 
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        // Could trigger chase animation or sound here
    }

    public override void Update()
    {
        if (enemy.Data == null)
        {
            return;
        }

        enemy.TickCombatCooldowns();

        // Check attack range based on behavior config
        bool shouldAttack = ShouldTransitionToAttack();
        if (shouldAttack)
        {
            stateMachine.ChangeState<EnemyAttackState>();
            return;
        }

        // Move toward player using movement pattern
        enemy.UpdateMovement();
    }

    /// <summary>
    /// Determine if enemy should transition to attack based on config.
    /// </summary>
    private bool ShouldTransitionToAttack()
    {
        // Ranged attackers transition when in attack range
        if (enemy.Data.CanFireProjectiles)
        {
            return enemy.IsPlayerInRange(enemy.Data.AttackRange);
        }

        // Spawners and melee enemies are always in attack mode while chasing
        // (spawning while moving, contact damage on collision)
        return true;
    }
}
