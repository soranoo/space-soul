using UnityEngine;

/// <summary>
/// Factory for creating and configuring enemies.
/// Implements Factory pattern for enemy spawning.
/// </summary>
public class EnemyFactory : SingletonBase<EnemyFactory>
{
    [Header("Default Settings")]
    [SerializeField] private EnemyData defaultEnemyData;

    /// <summary>
    /// Create an enemy from the pool with default data.
    /// </summary>
    /// <param name="position">Spawn position.</param>
    /// <param name="rotation">Spawn rotation.</param>
    /// <returns>Configured enemy, or null if spawn failed.</returns>
    public Enemy CreateEnemy(Vector3 position, Quaternion rotation)
    {
        return CreateEnemy(defaultEnemyData, position, rotation, 1, 1f, 1f);
    }

    /// <summary>
    /// Create an enemy from the pool with specified data.
    /// </summary>
    /// <param name="enemyData">Enemy type data.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="rotation">Spawn rotation.</param>
    /// <returns>Configured enemy, or null if spawn failed.</returns>
    public Enemy CreateEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        return CreateEnemy(enemyData, position, rotation, 1, 1f, 1f);
    }

    /// <summary>
    /// Create an enemy from the pool with wave-based scaling.
    /// </summary>
    /// <param name="enemyData">Enemy type data.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="rotation">Spawn rotation.</param>
    /// <param name="waveNumber">Current wave for stat scaling.</param>
    /// <param name="healthMultiplier">Additional health multiplier.</param>
    /// <param name="speedMultiplier">Additional speed multiplier.</param>
    /// <returns>Configured enemy, or null if spawn failed.</returns>
    public Enemy CreateEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation, 
        int waveNumber, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        if (enemyData == null)
        {
            return null;
        }

        Enemy enemy = null;

        // Try to get from pool
        if (PoolManager.Instance != null && enemyData.Prefab != null)
        {
            Enemy prefabComponent = enemyData.Prefab.GetComponent<Enemy>();
            if (prefabComponent != null)
            {
                enemy = PoolManager.Instance.Get(prefabComponent, position, rotation);
            }
            else
            {
                Debug.LogWarning("EnemyFactory: enemyData.Prefab does not have an Enemy component.");
            }
        }

        // Fallback to instantiation if pool not available
        if (enemy == null && enemyData.Prefab != null)
        {
            GameObject enemyObject = Instantiate(enemyData.Prefab, position, rotation);
            enemy = enemyObject.GetComponent<Enemy>();
        }
        else if (enemy == null && enemyData.Prefab == null)
        {
            Debug.LogError("[EnemyFactory] enemyData.Prefab is NULL!");
        }

        if (enemy == null)
        {
            Debug.LogWarning("EnemyFactory: Failed to create enemy. Check pool configuration or prefab.");
            return null;
        }

        // Calculate wave-based scaling
        float waveHealthMultiplier = CalculateWaveHealthMultiplier(waveNumber);
        float waveSpeedMultiplier = CalculateWaveSpeedMultiplier(waveNumber);

        // Apply combined multipliers
        float finalHealthMultiplier = healthMultiplier * waveHealthMultiplier;
        float finalSpeedMultiplier = speedMultiplier * waveSpeedMultiplier;

        // Initialize enemy with data and scaling
        enemy.Initialize(enemyData, finalHealthMultiplier, finalSpeedMultiplier);

        return enemy;
    }

    /// <summary>
    /// Calculate health multiplier based on wave number.
    /// </summary>
    /// <param name="waveNumber">Current wave.</param>
    /// <returns>Health multiplier.</returns>
    private float CalculateWaveHealthMultiplier(int waveNumber)
    {
        // Increase health by 10% per wave
        return 1f + (waveNumber - 1) * 0.1f;
    }

    /// <summary>
    /// Calculate speed multiplier based on wave number.
    /// </summary>
    /// <param name="waveNumber">Current wave.</param>
    /// <returns>Speed multiplier.</returns>
    private float CalculateWaveSpeedMultiplier(int waveNumber)
    {
        // Increase speed by 5% per wave, capped at 50% increase
        float multiplier = 1f + (waveNumber - 1) * 0.05f;
        return Mathf.Min(multiplier, 1.5f);
    }

    /// <summary>
    /// Create multiple enemies at random positions.
    /// </summary>
    /// <param name="enemyData">Enemy type data.</param>
    /// <param name="count">Number of enemies to spawn.</param>
    /// <param name="spawnCenter">Center point for spawning.</param>
    /// <param name="spawnRadius">Radius around center to spawn.</param>
    /// <param name="waveNumber">Current wave for scaling.</param>
    /// <returns>Array of spawned enemies.</returns>
    public Enemy[] CreateEnemies(EnemyData enemyData, int count, Vector3 spawnCenter, 
        float spawnRadius, int waveNumber)
    {
        Enemy[] enemies = new Enemy[count];

        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = spawnCenter + new Vector3(randomOffset.x, randomOffset.y, 0f);

            enemies[i] = CreateEnemy(enemyData, spawnPosition, Quaternion.identity, waveNumber);
        }

        return enemies;
    }

    /// <summary>
    /// Create enemies in a circle formation.
    /// </summary>
    /// <param name="enemyData">Enemy type data.</param>
    /// <param name="count">Number of enemies to spawn.</param>
    /// <param name="center">Center of the circle.</param>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="waveNumber">Current wave for scaling.</param>
    /// <returns>Array of spawned enemies.</returns>
    public Enemy[] CreateEnemiesInCircle(EnemyData enemyData, int count, Vector3 center, 
        float radius, int waveNumber)
    {
        Enemy[] enemies = new Enemy[count];
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            Vector3 spawnPosition = new Vector3(x, y, 0f);

            // Face toward center
            Vector2 directionToCenter = ((Vector2)center - new Vector2(x, y)).normalized;
            float rotation = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg - 90f;
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, rotation);

            enemies[i] = CreateEnemy(enemyData, spawnPosition, spawnRotation, waveNumber);
        }

        return enemies;
    }
}
