using UnityEngine;

/// <summary>
/// Weighted spawn entry for spawner enemies.
/// </summary>
[System.Serializable]
public class SpawnEntry
{
    [Tooltip("Enemy data to spawn.")]
    public EnemyData enemyData;

    [Tooltip("Spawn weight (higher = more likely).")]
    [Range(1, 100)]
    public int weight = 1;
}

/// <summary>
/// Defines enemy type properties using the Type Object pattern.
/// Enables data-driven enemy design with configurable behaviors.
/// Combine settings to create different enemy archetypes:
/// - Suicide: selfDestructOnContact=true, high speed, high contactDamage
/// - Attacker: canFireProjectiles=true, set attackRange/fireRate
/// - MotherShip: canSpawnEnemies=true, set spawnInterval/spawnData
/// - Soldier: default chase behavior, no special flags
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public enum RangedFireMode
    {
        OnlyInSafePosition,
        WhenDetected
    }

    [Header("Basic Stats")]
    [Tooltip("Base health points for this enemy type.")]
    [SerializeField] private int baseHealth = 1;

    [Tooltip("Base movement speed.")]
    [SerializeField] private float baseSpeed = 3f;

    [Tooltip("Damage dealt to player on contact.")]
    [SerializeField] private int contactDamage = 1;

    [Header("Range")]

    [Tooltip("Preferred distance to maintain from player (0 = chase to contact).")]
    [SerializeField] private float preferredDistance = 0f;

    [Header("Contact Behavior")]
    [Tooltip("If true, enemy dies on contact with player (suicide bomber style).")]
    [SerializeField] private bool selfDestructOnContact = false;

    [Header("Ranged Attack")]
    [Tooltip("If true, enemy can fire projectiles at player.")]
    [SerializeField] private bool canFireProjectiles = false;

    [Tooltip("Range at which enemy starts firing (if canFireProjectiles is true).")]
    [SerializeField] private float attackRange = 5f;

    [Tooltip("Fire rate (shots per second).")]
    [SerializeField] private float fireRate = 1f;

    [Tooltip("Projectile damage.")]
    [SerializeField] private int projectileDamage = 1;

    [Tooltip("Projectile speed.")]
    [SerializeField] private float projectileSpeed = 8f;

    [Tooltip("Custom projectile prefab (uses default if null).")]
    [SerializeField] private GameObject projectilePrefab;

    [Tooltip("How this enemy decides when it is allowed to fire.")]
    [SerializeField] private RangedFireMode fireMode = RangedFireMode.OnlyInSafePosition;

    [Header("Spawning Behavior")]
    [Tooltip("If true, enemy can spawn other enemies periodically.")]
    [SerializeField] private bool canSpawnEnemies = false;

    [Tooltip("Time between spawning enemies.")]
    [SerializeField] private float spawnInterval = 3f;

    [Tooltip("Number of enemies to spawn at once.")]
    [SerializeField] private int spawnCount = 1;

    [Tooltip("Maximum children this enemy can spawn in one life cycle. -1 = unlimited.")]
    [SerializeField] private int maxSpawnChildren = 30;

    [Tooltip("Weighted list of enemies to spawn.")]
    [SerializeField] private SpawnEntry[] spawnList;

    [Header("Rewards")]
    [Tooltip("Points awarded when destroyed.")]
    [SerializeField] private int pointValue = 100;

    [Tooltip("Chance to drop a power-up (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float powerUpDropChance = 0.1f;

    [Header("Visuals")]
    [Tooltip("Prefab for this enemy type.")]
    [SerializeField] private GameObject prefab;

    [Tooltip("If true, an alarm UI will be shown when this enemy spawns.")]
    [SerializeField] private bool showAlarmOnSpawn = false;

    [Tooltip("Damage tint color (applied briefly when hit).")]
    [SerializeField] private Color damageTintColor = Color.red;

    [Tooltip("Duration of damage tint (seconds).")]
    [SerializeField] private float damageTintDuration = 0.1f;

    // Basic Stats Properties
    public int BaseHealth => baseHealth;
    public float BaseSpeed => baseSpeed;
    public int ContactDamage => contactDamage;

    // Range Properties
    public float PreferredDistance => preferredDistance;

    // Contact Behavior Properties
    public bool SelfDestructOnContact => selfDestructOnContact;

    // Ranged Attack Properties
    public bool CanFireProjectiles => canFireProjectiles;
    public float AttackRange => attackRange;
    public float FireRate => fireRate;
    public int ProjectileDamage => projectileDamage;
    public float ProjectileSpeed => projectileSpeed;
    public GameObject ProjectilePrefab => projectilePrefab;
    public RangedFireMode FireMode => fireMode;

    // Spawning Behavior Properties
    public bool CanSpawnEnemies => canSpawnEnemies;
    public float SpawnInterval => spawnInterval;
    public int SpawnCount => spawnCount;
    public int MaxSpawnChildren => Mathf.Max(-1, maxSpawnChildren);
    public SpawnEntry[] SpawnList => spawnList;

    /// <summary>
    /// Get a random enemy data from spawn list based on weights.
    /// </summary>
    /// <returns>Selected EnemyData or null if list is empty.</returns>
    public EnemyData GetRandomSpawnData()
    {
        if (spawnList == null || spawnList.Length == 0)
        {
            return null;
        }

        int totalWeight = 0;
        for (int i = 0; i < spawnList.Length; i++)
        {
            if (spawnList[i] != null && spawnList[i].enemyData != null)
            {
                totalWeight += spawnList[i].weight;
            }
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulative = 0;

        for (int i = 0; i < spawnList.Length; i++)
        {
            if (spawnList[i] != null && spawnList[i].enemyData != null)
            {
                cumulative += spawnList[i].weight;
                if (randomValue < cumulative)
                {
                    return spawnList[i].enemyData;
                }
            }
        }

        return spawnList[0]?.enemyData;
    }

    // Rewards Properties
    public int PointValue => pointValue;
    public int ScoreValue => pointValue;
    public float PowerUpDropChance => powerUpDropChance;

    // Visual Properties
    public GameObject Prefab => prefab;
    public bool ShowAlarmOnSpawn => showAlarmOnSpawn;
    public Color DamageTintColor => damageTintColor;
    public float DamageTintDuration => damageTintDuration;
}
