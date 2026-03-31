using UnityEngine;

/// <summary>
/// Attack state - enemy engages the player based on configured behaviour.
/// Behaviours are determined by EnemyData flags:
/// - selfDestructOnContact: Chase at full speed until contact
/// - canFireProjectiles: Maintain distance and fire
/// - canSpawnEnemies: Advance and spawn child enemies
/// - Default: Chase and contact attack
/// </summary>
public class EnemyAttackState : BaseEnemyState
{
    /// <summary>
    /// Maximum angle offset (degrees) to allow firing. 
    /// Enemy must be within this angle of facing the player to shoot.
    /// </summary>
    private const float FIRE_ANGLE_TOLERANCE = 15f;

    private const float CHASE_TO_ATTACK_RANGE_MULTIPLIER = 1.5f;
    private const float RETREAT_DISTANCE_MULTIPLIER = 0.5f;

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

        enemy.TickCombatCooldowns();

        // Handle spawning behaviour
        if (enemy.Data.CanSpawnEnemies && enemy.CanSpawnEnemies())
        {
            enemy.SpawnChildEnemies();
        }

        // Handle ranged attack behaviour
        if (enemy.Data.CanFireProjectiles)
        {
            UpdateRangedAttacker();
            return;
        }

        // Default: Chase behaviour (suicide, spawner, or basic melee)
        // Contact damage handled by Enemy.OnTriggerEnter2D
        enemy.UpdateMovement();
    }

    /// <summary>
    /// Ranged attacker behaviour: Maintain preferred distance, rotate to face and fire at player.
    /// Enemy must be facing the player before it can fire.
    /// When too close, the enemy turns away and flies to create distance before re-engaging.
    /// </summary>
    private void UpdateRangedAttacker()
    {
        float distance = enemy.GetDistanceToPlayer();
        float attackRange = enemy.Data.AttackRange;
        float preferredDist = enemy.Data.PreferredDistance;
        bool whenDetectedMode = enemy.Data.FireMode == EnemyData.RangedFireMode.WhenDetected;

        // Use preferredDistance if set, otherwise use attackRange
        float targetDistance = preferredDist > 0f ? preferredDist : attackRange;

        if (whenDetectedMode)
        {
            enemy.RotateTowardPlayer();

            if (distance > targetDistance * CHASE_TO_ATTACK_RANGE_MULTIPLIER)
            {
                stateMachine.ChangeState<EnemyChaseState>();
                return;
            }

            if (distance < targetDistance * RETREAT_DISTANCE_MULTIPLIER)
            {
                enemy.RotateAwayFromPlayer();
                enemy.MoveForward(enemy.EffectiveSpeed * RETREAT_DISTANCE_MULTIPLIER);
            }

            if (enemy.CanFireAtDistance(distance))
            {
                enemy.FireAtPlayer();
            }

            return;
        }

        // If too far, chase
        if (distance > targetDistance * CHASE_TO_ATTACK_RANGE_MULTIPLIER)
        {
            stateMachine.ChangeState<EnemyChaseState>();
            return;
        }

        // If too close, retreat: rotate away from player and fly forward
        if (distance < targetDistance * RETREAT_DISTANCE_MULTIPLIER)
        {
            enemy.RotateAwayFromPlayer();
            enemy.MoveForward(enemy.EffectiveSpeed * RETREAT_DISTANCE_MULTIPLIER);
            return; // Don't fire while retreating
        }

        // If at good distance, rotate to face the player and fire
        enemy.RotateTowardPlayer();

        // Only fire when allowed by fire mode and facing the player
        if (enemy.CanFireAtDistance(distance) && enemy.IsFacingPlayer(FIRE_ANGLE_TOLERANCE))
        {
            enemy.FireAtPlayer();
        }
    }
}
