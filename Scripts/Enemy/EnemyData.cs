using UnityEngine;

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
    [Header("Basic Stats")]
    [Tooltip("Base health points for this enemy type.")]
    [SerializeField] private int baseHealth = 1;

    [Tooltip("Base movement speed.")]
    [SerializeField] private float baseSpeed = 3f;

    [Tooltip("Damage dealt to player on contact.")]
    [SerializeField] private int contactDamage = 1;

    [Header("Detection & Range")]
    [Tooltip("Detection range for targeting player.")]
    [SerializeField] private float detectionRange = 15f;

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

    [Header("Spawning Behavior")]
    [Tooltip("If true, enemy can spawn other enemies periodically.")]
    [SerializeField] private bool canSpawnEnemies = false;

    [Tooltip("Time between spawning enemies.")]
    [SerializeField] private float spawnInterval = 3f;

    [Tooltip("Number of enemies to spawn at once.")]
    [SerializeField] private int spawnCount = 1;

    [Tooltip("EnemyData for spawned enemies.")]
    [SerializeField] private EnemyData spawnedEnemyData;

    [Header("Rewards")]
    [Tooltip("Points awarded when destroyed.")]
    [SerializeField] private int pointValue = 100;

    [Tooltip("Chance to drop a power-up (0-1).")]
    [Range(0f, 1f)]
    [SerializeField] private float powerUpDropChance = 0.1f;

    [Header("Visuals")]
    [Tooltip("Prefab for this enemy type.")]
    [SerializeField] private GameObject prefab;

    [Tooltip("Damage tint color (applied briefly when hit).")]
    [SerializeField] private Color damageTintColor = Color.red;

    [Tooltip("Duration of damage tint (seconds).")]
    [SerializeField] private float damageTintDuration = 0.1f;

    // Basic Stats Properties
    public int BaseHealth => baseHealth;
    public float BaseSpeed => baseSpeed;
    public int ContactDamage => contactDamage;

    // Detection & Range Properties
    public float DetectionRange => detectionRange;
    public float PreferredDistance => preferredDistance;

    // Contact Behavior Properties
    public bool SelfDestructOnContact => selfDestructOnContact;

    // Ranged Attack Properties
    public bool CanFireProjectiles => canFireProjectiles;
    public float AttackRange => attackRange;
    public float FireRate => fireRate;
    public int ProjectileDamage => projectileDamage;
    public float ProjectileSpeed => projectileSpeed;

    // Spawning Behavior Properties
    public bool CanSpawnEnemies => canSpawnEnemies;
    public float SpawnInterval => spawnInterval;
    public int SpawnCount => spawnCount;
    public EnemyData SpawnedEnemyData => spawnedEnemyData;

    // Rewards Properties
    public int PointValue => pointValue;
    public float PowerUpDropChance => powerUpDropChance;

    // Visual Properties
    public GameObject Prefab => prefab;
    public Color DamageTintColor => damageTintColor;
    public float DamageTintDuration => damageTintDuration;
}
