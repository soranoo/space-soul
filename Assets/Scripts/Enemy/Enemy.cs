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
    [SerializeField] private string poolIdOverride;
    private const string ANIM_TRIGGER_DEAD = "Dead";
    private const string ANIM_TRIGGER_FIRE = "Fire";

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private Collider2D hitCollider;
    [SerializeField] private GameObject engine;

    [Header("Audio")]
    [SerializeField] private AudioSettings engineSfx;
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField, Min(0f)] private float movementSfxThreshold = 0.0001f;

    private EnemyData data;
    private EnemyStateMachine stateMachine;
    private IMovementPattern movementPattern;
    private Transform playerTransform;

    [SerializeField] private int currentHealth;
    private float effectiveSpeed;
    private Color defaultColor;
    private Coroutine damageTintCoroutine;
    private bool isDead;
    private Vector3 previousPosition;

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

    /// <summary>
    /// Get the pool identifier for this prefab type.
    /// </summary>
    public string GetPoolId()
    {
        if (!string.IsNullOrWhiteSpace(poolIdOverride))
        {
            return poolIdOverride;
        }

        return gameObject.name;
    }

    /// <summary>
    /// Assign the pool identifier for this instance.
    /// </summary>
    public void SetPoolId(string poolId)
    {
        poolIdOverride = poolId;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer != null)
        {
            defaultColor = spriteRenderer.color;
        }

        if (engineAudioSource == null)
        {
            engineAudioSource = GetComponent<AudioSource>();

            if (engineAudioSource == null)
            {
                engineAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        ConfigureEngineAudioSource();
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
        isDead = false;

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
        }

        // Reset visual tint
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = defaultColor;
        }

        if (animator != null)
        {
            animator.enabled = true;
        }

        if (engine != null)
        {
            engine.SetActive(true);
        }

        // Setup movement pattern (always chase)
        SetupMovementPattern();

        // Setup state machine based on role
        SetupStateMachine();

        // Find player reference
        FindPlayer();

        previousPosition = transform.position;
        StopEngineAudio();
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
        isDead = false;

        if (damageTintCoroutine != null)
        {
            StopCoroutine(damageTintCoroutine);
            damageTintCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = defaultColor;
        }

        if (animator != null)
        {
            animator.enabled = true;
        }

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
        }

        if (engine != null)
        {
            engine.SetActive(true);
        }

        StopEngineAudio();
        previousPosition = transform.position;

    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

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

        UpdateEngineAudioByMovement();

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
        if (!isDead && movementPattern != null)
        {
            movementPattern.UpdateMovement(transform, effectiveSpeed);
        }
    }

    /// <summary>
    /// Smoothly rotate the enemy to face the player.
    /// Uses a rotation speed proportional to effective speed for natural feel.
    /// </summary>
    /// <param name="rotationSpeed">Degrees per second to rotate. 0 = instant.</param>
    public void RotateTowardPlayer(float rotationSpeed = 360f)
    {
        if (playerTransform == null || isDead)
        {
            return;
        }

        Vector2 direction = GetDirectionToPlayer();
        if (direction == Vector2.zero)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        if (rotationSpeed <= 0f)
        {
            // Instant rotation
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Smoothly rotate the enemy to face away from the player (for retreating).
    /// </summary>
    /// <param name="rotationSpeed">Degrees per second to rotate. 0 = instant.</param>
    public void RotateAwayFromPlayer(float rotationSpeed = 360f)
    {
        if (playerTransform == null || isDead)
        {
            return;
        }

        Vector2 direction = -GetDirectionToPlayer();
        if (direction == Vector2.zero)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        if (rotationSpeed <= 0f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Move the enemy forward in its facing direction (transform.up).
    /// </summary>
    /// <param name="speed">Movement speed.</param>
    public void MoveForward(float speed)
    {
        if (isDead)
        {
            return;
        }

        transform.Translate(Vector2.up * speed * Time.deltaTime, Space.Self);
    }

    /// <summary>
    /// Check if this enemy is facing the player within a given angle threshold.
    /// </summary>
    /// <param name="angleTolerance">Maximum angle (degrees) offset allowed to count as "facing".</param>
    /// <returns>True if the enemy's forward (up) direction is within the tolerance of the player direction.</returns>
    public bool IsFacingPlayer(float angleTolerance = 15f)
    {
        if (playerTransform == null)
        {
            return false;
        }

        Vector2 direction = GetDirectionToPlayer();
        if (direction == Vector2.zero)
        {
            return false;
        }

        // Enemy's forward in 2D is transform.up
        float angle = Vector2.Angle(transform.up, direction);
        return angle <= angleTolerance;
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

        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_FIRE);
        }

        // Set cooldown
        attackCooldown = 1f / data.FireRate;

        // Spawn projectile - this will be handled by a projectile system
        // For now, emit event or spawn directly
        SpawnEnemyProjectile();
    }

    /// <summary>
    /// Spawn enemy projectile in the direction the enemy is facing (transform.up).
    /// The enemy must be rotated to face the target before calling this.
    /// </summary>
    private void SpawnEnemyProjectile()
    {
        if (data.ProjectilePrefab == null)
        {
            Debug.LogWarning("[Enemy] No projectile prefab assigned in EnemyData.");
            return;
        }

        // Fire in the direction the enemy is visually facing
        Vector2 direction = transform.up;

        // Spawn position slightly in front of enemy
        Vector3 spawnPos = transform.position + (Vector3)(direction * 0.5f);

        // Try to get from pool first
        EnemyProjectile projectile = null;

        if (PoolManager.Instance != null)
        {
            EnemyProjectile prefabComponent = data.ProjectilePrefab.GetComponent<EnemyProjectile>();
            if (prefabComponent != null)
            {
                projectile = PoolManager.Instance.Get(prefabComponent, spawnPos, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("[Enemy] Projectile prefab does not have an EnemyProjectile component.");
            }
        }

        // If no pool or pool empty, instantiate
        if (projectile == null)
        {
            GameObject projObj = Instantiate(data.ProjectilePrefab, spawnPos, Quaternion.identity);
            projectile = projObj.GetComponent<EnemyProjectile>();
        }
        else if (projectile != null)
        {
            projectile.transform.position = spawnPos;
            projectile.gameObject.SetActive(true);
        }

        if (projectile != null)
        {
            projectile.Initialize(data.ProjectileDamage, data.ProjectileSpeed, direction);
        }
    }

    /// <summary>
    /// Check if can spawn enemies (spawning enabled).
    /// </summary>
    /// <returns>True if can spawn.</returns>
    public bool CanSpawnEnemies()
    {
        return spawnCooldown <= 0f && data != null && data.CanSpawnEnemies && data.SpawnList != null && data.SpawnList.Length > 0;
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

        // Spawn each child using weighted random selection
        for (int i = 0; i < data.SpawnCount; i++)
        {
            EnemyData spawnData = data.GetRandomSpawnData();
            if (spawnData != null)
            {
                SpawnSoldiersRequested?.Invoke(this, spawnData, 1);
            }
        }
    }

    [Header("Audio")]
    [SerializeField] private AudioSettings selfDestructSfx;

    /// <summary>
    /// Execute self-destruct behavior - deal damage and die.
    /// </summary>
    public void ExecuteSelfDestruct(PlayerController player)
    {
        if (player != null && data != null)
        {
            player.TakeDamage(data.ContactDamage);
        }

        SfxManager.Instance?.Play(selfDestructSfx);
        Die();
    }

    /// <summary>
    /// Apply damage to the enemy.
    /// </summary>
    /// <param name="amount">Damage amount.</param>
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead)
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
    /// Fires Died event but does NOT destroy - external animation handler should call Cleanup() when done.
    /// </summary>
    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        if (engine != null)
        {
            engine.SetActive(false);
        }

        StopEngineAudio();

        Died?.Invoke(this);
        if (animator != null)
        {
            animator.SetTrigger(ANIM_TRIGGER_DEAD);
        }
        // Do not destroy here - let death animation handler call Cleanup() when finished
    }

    private void ConfigureEngineAudioSource()
    {
        if (engineAudioSource == null)
        {
            return;
        }

        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = true;

        if (engineSfx != null)
        {
            engineAudioSource.clip = engineSfx.Clip;
            engineSfx.Source?.ApplyTo(engineAudioSource);
            engineAudioSource.loop = true;
        }
    }

    private void UpdateEngineAudioByMovement()
    {
        if (engineAudioSource == null || engineSfx == null || engineSfx.Clip == null)
        {
            StopEngineAudio();
            previousPosition = transform.position;
            return;
        }

        float movedSqr = ((Vector2)(transform.position - previousPosition)).sqrMagnitude;
        bool isMoving = movedSqr > movementSfxThreshold;

        if (isMoving)
        {
            PlayEngineAudio();
        }
        else
        {
            StopEngineAudio();
        }

        previousPosition = transform.position;
    }

    private void PlayEngineAudio()
    {
        if (engineAudioSource == null || engineSfx == null || engineSfx.Clip == null)
        {
            return;
        }

        if (engineAudioSource.clip != engineSfx.Clip)
        {
            engineAudioSource.clip = engineSfx.Clip;
        }

        if (!engineAudioSource.isPlaying)
        {
            engineSfx.Source?.ApplyTo(engineAudioSource);
            engineAudioSource.loop = true;
            engineAudioSource.Play();
        }
    }

    private void StopEngineAudio()
    {
        if (engineAudioSource != null && engineAudioSource.isPlaying)
        {
            engineAudioSource.Stop();
        }
    }

    /// <summary>
    /// Clean up and return enemy to pool. Call this after death animation completes.
    /// </summary>
    public void Cleanup()
    {
        if (damageTintCoroutine != null)
        {
            StopCoroutine(damageTintCoroutine);
            damageTintCoroutine = null;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        PoolManager.Instance.Release(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (data == null) {
            return;
        }

        if (other.GetComponent<ShieldBlocker>() != null && data.SelfDestructOnContact)
        {
            // If we hit a shield blocker and are a self-destruct type, just execute self-destruct without damaging player (since shield blocks it)
            ExecuteSelfDestruct(null);
            return;
        }

        // Handle collision with player
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                // If we can't find player, just ignore collision
                return;
            }
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

        // Handle collision with bullets
        if (other.CompareTag("PlayerProjectile"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.GetDamage());
            }
        }
    }
}
