using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Base enemy behavior.
/// Implements IPoolable for object pooling support.
/// </summary>
public class Enemy : MonoBehaviour, IPoolable
{
    public const string POOL_ID = "Enemy";

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private EnemyData data;
    private EnemyStateMachine stateMachine;
    private IMovementPattern movementPattern;
    private Transform playerTransform;

    [SerializeField] private int currentHealth;
    private float effectiveSpeed;
    private Color defaultColor;
    private Coroutine damageTintCoroutine;

    // Role-specific variables
    private float attackCooldown;
    private float spawnCooldown;

    /// <summary>
    /// Enemy data configuration.
    /// </summary>
    public EnemyData Data => data;

    /// <summary>
    /// Current health points.
    /// </summary>
    public int CurrentHealth => currentHealth;

    /// <summary>
    /// Effective movement speed after scaling.
    /// </summary>
    public float EffectiveSpeed => effectiveSpeed;

    /// <summary>
    /// Player transform reference.
    /// </summary>
    public Transform PlayerTransform => playerTransform;

    /// <summary>
    /// Event fired when enemy dies.
    /// </summary>
    public event Action<Enemy> Died;

    /// <summary>
    /// Event fired when enemy takes damage.
    /// </summary>
    public event Action<Enemy, int> DamageTaken;

    /// <summary>
    /// Event fired when MotherShip wants to spawn soldiers.
    /// </summary>
    public event Action<Enemy, EnemyData, int> SpawnSoldiersRequested;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Initialize the enemy with data and scaling.
    /// </summary>
    /// <param name="enemyData">Data configuration for this enemy type.</param>
    /// <param name="healthMultiplier">Health scaling multiplier.</param>
    /// <param name="speedMultiplier">Speed scaling multiplier.</param>
    public void Initialize(EnemyData enemyData, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        data = enemyData;
        currentHealth = Mathf.RoundToInt(data.BaseHealth * healthMultiplier);
        effectiveSpeed = data.BaseSpeed * speedMultiplier;
        attackCooldown = 0f;
        spawnCooldown = 0f;

        // Reset visual tint
        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }

        // Setup movement pattern (always chase)
        SetupMovementPattern();

        // Setup state machine based on role
        SetupStateMachine();

        // Find player reference
        FindPlayer();
    }

    /// <summary>
    /// Setup the movement pattern - always chase player.
    /// </summary>
    private void SetupMovementPattern()
    {
        movementPattern = new ChasePlayerPattern();
        movementPattern.Initialize(transform);
    }

    /// <summary>
    /// Setup the enemy state machine based on role.
    /// </summary>
    private void SetupStateMachine()
    {
        stateMachine = new EnemyStateMachine();
        stateMachine.RegisterState(new EnemyIdleState(this, stateMachine));
        stateMachine.RegisterState(new EnemyChaseState(this, stateMachine));
        stateMachine.RegisterState(new EnemyAttackState(this, stateMachine));

        // Start in idle state
        stateMachine.ChangeState<EnemyIdleState>();
    }

    /// <summary>
    /// Find the player in the scene.
    /// </summary>
    private void FindPlayer()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// Called when retrieved from pool.
    /// </summary>
    public void OnSpawn()
    {
        // Reset will be called after Initialize
    }

    /// <summary>
    /// Called when returned to pool.
    /// </summary>
    public void OnDespawn()
    {
        if (stateMachine != null)
        {
            stateMachine.Reset();
        }

        if (movementPattern != null)
        {
            movementPattern.Reset();
        }

        playerTransform = null;
        data = null;
        attackCooldown = 0f;
        spawnCooldown = 0f;

        if (damageTintCoroutine != null)
        {
            StopCoroutine(damageTintCoroutine);
            damageTintCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = defaultColor;
        }
    }

    private void Update()
    {
        if (stateMachine != null)
        {
            stateMachine.Update();
        }

        // Update cooldowns
        if (attackCooldown > 0f)
        {
            attackCooldown -= Time.deltaTime;
        }

        if (spawnCooldown > 0f)
        {
            spawnCooldown -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Check if player is within specified range.
    /// </summary>
    /// <param name="range">Range to check.</param>
    /// <returns>True if player is within range.</returns>
    public bool IsPlayerInRange(float range)
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null)
            {
                return false;
            }
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= range;
    }

    /// <summary>
    /// Get distance to player.
    /// </summary>
    /// <returns>Distance to player.</returns>
    public float GetDistanceToPlayer()
    {
        if (playerTransform == null)
        {
            return float.MaxValue;
        }

        return Vector2.Distance(transform.position, playerTransform.position);
    }

    /// <summary>
    /// Get direction to player.
    /// </summary>
    /// <returns>Normalized direction vector to player.</returns>
    public Vector2 GetDirectionToPlayer()
    {
        if (playerTransform == null)
        {
            return Vector2.zero;
        }

        return ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;
    }

    /// <summary>
    /// Update movement using the chase pattern.
    /// </summary>
    public void UpdateMovement()
    {
        if (movementPattern != null)
        {
            movementPattern.UpdateMovement(transform, effectiveSpeed);
        }
    }

    /// <summary>
    /// Check if can fire (has ranged attack enabled).
    /// </summary>
    /// <returns>True if can fire.</returns>
    public bool CanFire()
    {
        return attackCooldown <= 0f && data != null && data.CanFireProjectiles;
    }

    /// <summary>
    /// Fire projectile at player.
    /// </summary>
    public void FireAtPlayer()
    {
        if (!CanFire() || playerTransform == null)
        {
            return;
        }

        // Set cooldown
        attackCooldown = 1f / data.FireRate;

        // Spawn projectile - this will be handled by a projectile system
        // For now, emit event or spawn directly
        SpawnEnemyProjectile();
    }

    /// <summary>
    /// Spawn enemy projectile toward player.
    /// </summary>
    private void SpawnEnemyProjectile()
    {
        // Get direction to player
        Vector2 direction = GetDirectionToPlayer();

        // Spawn bullet from pool or instantiate
        // TODO: Integrate with bullet pool when enemy bullets are implemented
        Debug.Log($"[Enemy] Firing projectile at player");
    }

    /// <summary>
    /// Check if can spawn enemies (spawning enabled).
    /// </summary>
    /// <returns>True if can spawn.</returns>
    public bool CanSpawnEnemies()
    {
        return spawnCooldown <= 0f && data != null && data.CanSpawnEnemies && data.SpawnedEnemyData != null;
    }

    /// <summary>
    /// Spawn child enemies.
    /// </summary>
    public void SpawnChildEnemies()
    {
        if (!CanSpawnEnemies())
        {
            return;
        }

        // Set cooldown
        spawnCooldown = data.SpawnInterval;

        // Request spawning through event
        SpawnSoldiersRequested?.Invoke(this, data.SpawnedEnemyData, data.SpawnCount);
    }

    /// <summary>
    /// Execute self-destruct behavior - deal damage and die.
    /// </summary>
    public void ExecuteSelfDestruct(PlayerController player)
    {
        if (player != null && data != null)
        {
            player.TakeDamage(data.ContactDamage);
        }

        Die();
    }

    /// <summary>
    /// Apply damage to the enemy.
    /// </summary>
    /// <param name="amount">Damage amount.</param>
    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth -= amount;
        DamageTaken?.Invoke(this, amount);

        FlashDamageTint();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Flash a damage tint for a short duration.
    /// </summary>
    private void FlashDamageTint()
    {
        if (spriteRenderer == null || data == null)
        {
            return;
        }

        if (data.DamageTintDuration <= 0f)
        {
            return;
        }

        if (damageTintCoroutine != null)
        {
            StopCoroutine(damageTintCoroutine);
        }

        damageTintCoroutine = StartCoroutine(DamageTintRoutine());
    }

    private IEnumerator DamageTintRoutine()
    {
        spriteRenderer.color = data.DamageTintColor;
        yield return new WaitForSeconds(data.DamageTintDuration);
        spriteRenderer.color = defaultColor;
        damageTintCoroutine = null;
    }

    /// <summary>
    /// Handle enemy death.
    /// </summary>
    private void Die()
    {
        Died?.Invoke(this);

        // Return to pool
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(POOL_ID, this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Handle collision with player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && data != null)
            {
                // Self-destruct enemies die on contact
                if (data.SelfDestructOnContact)
                {
                    ExecuteSelfDestruct(player);
                }
                else
                {
                    // Other enemies just deal contact damage
                    player.TakeDamage(data.ContactDamage);
                }
            }
        }

        // Handle collision with bullets
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.GetDamage());
            }
        }
    }
}
