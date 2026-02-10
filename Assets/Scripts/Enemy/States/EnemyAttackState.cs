using UnityEngine;

/// <summary>
/// Attack state - enemy engages the player based on configured behavior.
/// Behaviors are determined by EnemyData flags:
/// - selfDestructOnContact: Chase at full speed until contact
/// - canFireProjectiles: Maintain distance and fire
/// - canSpawnEnemies: Advance and spawn child enemies
/// - Default: Chase and contact attack
/// </summary>
public class EnemyAttackState : BaseEnemyState
{
    public EnemyAttackState(Enemy enemy, EnemyStateMachine stateMachine) 
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
    }

    public override void Update()
    {
        if (enemy.Data == null)
        {
            return;
        }

        // Handle spawning behavior (can combine with other behaviors)
        if (enemy.Data.CanSpawnEnemies && enemy.CanSpawnEnemies())
        {
            enemy.SpawnChildEnemies();
        }

        // Handle ranged attack behavior
        if (enemy.Data.CanFireProjectiles)
        {
            UpdateRangedAttacker();
            return;
        }

        // Default: Chase behavior (suicide, spawner, or basic melee)
        // Contact damage handled by Enemy.OnTriggerEnter2D
        enemy.UpdateMovement();
    }

    /// <summary>
    /// Ranged attacker behavior: Maintain preferred distance and fire at player.
    /// </summary>
    private void UpdateRangedAttacker()
    {
        float distance = enemy.GetDistanceToPlayer();
        float attackRange = enemy.Data.AttackRange;
        float preferredDist = enemy.Data.PreferredDistance;

        // Use preferredDistance if set, otherwise use attackRange
        float targetDistance = preferredDist > 0f ? preferredDist : attackRange;

        // If too far, chase
        if (distance > targetDistance * 1.5f)
        {
            stateMachine.ChangeState<EnemyChaseState>();
            return;
        }

        // If too close, back away
        if (distance < targetDistance * 0.5f)
        {
            Vector2 awayDir = -enemy.GetDirectionToPlayer();
            enemy.transform.Translate(awayDir * enemy.EffectiveSpeed * 0.5f * Time.deltaTime, Space.World);
        }

        // Fire at player when in range
        if (distance <= attackRange && enemy.CanFire())
        {
            enemy.FireAtPlayer();
        }
    }
}
