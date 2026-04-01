using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Calculates per-wave scaling for enemy stats.
/// Provides multipliers for health, speed, damage, and enemy count.
/// </summary>
[Serializable]
public class DifficultyScaler : MonoBehaviour
{
    [Header("Enemy Sources")]
    [Tooltip("Enemy types available for difficulty-based wave generation.")]
    [SerializeField] private EnemyData[] enemyData;

    [Header("Wave Difficulty")]
    [Tooltip("Wave target increment. Target difficulty = last difficulty + difficulty multiplier * wave.")]
    [SerializeField] private float difficultyMultiplier = 1f;

    [Tooltip("Actual difficulty reached in the last completed wave.")]
    [SerializeField] private float lastWaveDifficulty = 0f;

    [Header("Health Scaling")]
    [Tooltip("Base health multiplier.")]
    [SerializeField] private float baseHealthMultiplier = 1f;

    [Tooltip("Additional health multiplier per wave.")]
    [SerializeField] private float healthIncreasePerWave = 0.1f;

    [Tooltip("Maximum health multiplier cap.")]
    [SerializeField] private float maxHealthMultiplier = 5f;

    [Header("Speed Scaling")]
    [Tooltip("Base speed multiplier.")]
    [SerializeField] private float baseSpeedMultiplier = 1f;

    [Tooltip("Additional speed multiplier per wave.")]
    [SerializeField] private float speedIncreasePerWave = 0.05f;

    [Tooltip("Maximum speed multiplier cap.")]
    [SerializeField] private float maxSpeedMultiplier = 2f;

    [Header("Damage Scaling")]
    [Tooltip("Base damage multiplier.")]
    [SerializeField] private float baseDamageMultiplier = 1f;

    [Tooltip("Additional damage multiplier per wave.")]
    [SerializeField] private float damageIncreasePerWave = 0.1f;

    [Tooltip("Maximum damage multiplier cap.")]
    [SerializeField] private float maxDamageMultiplier = 3f;

    [Header("Enemy Count Scaling")]
    [Tooltip("Base enemy count.")]
    [SerializeField] private int baseEnemyCount = 5;

    [Tooltip("Additional enemies per wave.")]
    [SerializeField] private int enemyIncreasePerWave = 2;

    [Tooltip("Maximum enemy count cap.")]
    [SerializeField] private int maxEnemyCount = 50;

    [Header("Spawn Rate Scaling")]
    [Tooltip("Base spawn interval in seconds.")]
    [SerializeField] private float baseSpawnInterval = 0.5f;

    [Tooltip("Spawn interval reduction per wave.")]
    [SerializeField] private float spawnIntervalDecreasePerWave = 0.02f;

    [Tooltip("Minimum spawn interval.")]
    [SerializeField] private float minSpawnInterval = 0.1f;

    /// <summary>
    /// Difficulty multiplier used in target wave difficulty calculation.
    /// </summary>
    public float DifficultyMultiplier => difficultyMultiplier;

    /// <summary>
    /// Actual difficulty value of the last completed wave.
    /// </summary>
    public float LastWaveDifficulty => lastWaveDifficulty;

    /// <summary>
    /// Get health multiplier for a specific wave.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Health multiplier.</returns>
    public float GetHealthMultiplier(int waveNumber)
    {
        if (waveNumber <= 0)
        {
            return baseHealthMultiplier;
        }

        float multiplier = baseHealthMultiplier + (waveNumber - 1) * healthIncreasePerWave;
        return Mathf.Min(multiplier, maxHealthMultiplier);
    }

    /// <summary>
    /// Get speed multiplier for a specific wave.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Speed multiplier.</returns>
    public float GetSpeedMultiplier(int waveNumber)
    {
        if (waveNumber <= 0)
        {
            return baseSpeedMultiplier;
        }

        float multiplier = baseSpeedMultiplier + (waveNumber - 1) * speedIncreasePerWave;
        return Mathf.Min(multiplier, maxSpeedMultiplier);
    }

    /// <summary>
    /// Get damage multiplier for a specific wave.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Damage multiplier.</returns>
    public float GetDamageMultiplier(int waveNumber)
    {
        if (waveNumber <= 0)
        {
            return baseDamageMultiplier;
        }

        float multiplier = baseDamageMultiplier + (waveNumber - 1) * damageIncreasePerWave;
        return Mathf.Min(multiplier, maxDamageMultiplier);
    }

    /// <summary>
    /// Get enemy count for a specific wave.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Number of enemies to spawn.</returns>
    public int GetEnemyCount(int waveNumber)
    {
        if (waveNumber <= 0)
        {
            return baseEnemyCount;
        }

        int count = baseEnemyCount + (waveNumber - 1) * enemyIncreasePerWave;
        return Mathf.Min(count, maxEnemyCount);
    }

    /// <summary>
    /// Get spawn interval for a specific wave.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Spawn interval in seconds.</returns>
    public float GetSpawnInterval(int waveNumber)
    {
        if (waveNumber <= 0)
        {
            return baseSpawnInterval;
        }

        float interval = baseSpawnInterval - (waveNumber - 1) * spawnIntervalDecreasePerWave;
        return Mathf.Max(interval, minSpawnInterval);
    }

    /// <summary>
    /// Get all difficulty values for a specific wave.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Struct containing all difficulty values.</returns>
    public WaveDifficultyValues GetWaveDifficulty(int waveNumber)
    {
        return new WaveDifficultyValues
        {
            healthMultiplier = GetHealthMultiplier(waveNumber),
            speedMultiplier = GetSpeedMultiplier(waveNumber),
            damageMultiplier = GetDamageMultiplier(waveNumber),
            enemyCount = GetEnemyCount(waveNumber),
            spawnInterval = GetSpawnInterval(waveNumber)
        };
    }

    /// <summary>
    /// Calculate target difficulty for a wave.
    /// Formula: lastDifficulty + (difficultyMultiplier * waveNumber)
    /// </summary>
    public float GetTargetWaveDifficulty(int waveNumber)
    {
        int safeWave = Mathf.Max(1, waveNumber);
        return Mathf.Max(0f, lastWaveDifficulty + (difficultyMultiplier * safeWave));
    }

    /// <summary>
    /// Update the last completed wave difficulty.
    /// </summary>
    public void SetLastWaveDifficulty(float difficulty)
    {
        lastWaveDifficulty = Mathf.Max(0f, difficulty);
    }

    /// <summary>
    /// Sum difficulty factors from a wave config.
    /// </summary>
    public static float CalculateWaveDifficultyFromConfig(WaveConfig config)
    {
        if (config == null)
        {
            return 0f;
        }

        return CalculateWaveDifficultyFromEntries(config.EnemySpawns);
    }

    /// <summary>
    /// Sum difficulty factors from wave spawn entries.
    /// </summary>
    public static float CalculateWaveDifficultyFromEntries(EnemySpawnEntry[] entries)
    {
        if (entries == null)
        {
            return 0f;
        }

        float totalDifficulty = 0f;
        for (int i = 0; i < entries.Length; i++)
        {
            EnemySpawnEntry entry = entries[i];
            if (entry == null || entry.enemyData == null || entry.count <= 0)
            {
                continue;
            }

            totalDifficulty += Mathf.Max(0.01f, entry.enemyData.DifficultyFactor) * entry.count;
        }

        return totalDifficulty;
    }

    /// <summary>
    /// Build an enemy composition that reaches the target difficulty with as few enemies as possible.
    /// </summary>
    public EnemyData[] CalculateEnemyComposition(float targetDifficulty)
    {
        if (targetDifficulty <= 0f)
        {
            return Array.Empty<EnemyData>();
        }

        List<EnemyData> sourcePool = GetValidEnemyPool();
        if (sourcePool.Count == 0)
        {
            return Array.Empty<EnemyData>();
        }

        sourcePool.Sort((a, b) => b.DifficultyFactor.CompareTo(a.DifficultyFactor));

        List<EnemyData> composition = new List<EnemyData>();
        float remaining = targetDifficulty;

        for (int i = 0; i < sourcePool.Count; i++)
        {
            EnemyData data = sourcePool[i];
            float factor = Mathf.Max(0.01f, data.DifficultyFactor);
            int count = Mathf.FloorToInt(remaining / factor);

            for (int j = 0; j < count; j++)
            {
                composition.Add(data);
            }

            remaining -= count * factor;
            if (remaining <= 0f)
            {
                break;
            }
        }

        if (remaining > 0f)
        {
            EnemyData bestCandidate = sourcePool[0];
            float lowestOvershoot = float.MaxValue;

            for (int i = 0; i < sourcePool.Count; i++)
            {
                EnemyData data = sourcePool[i];
                float factor = Mathf.Max(0.01f, data.DifficultyFactor);

                if (factor < remaining)
                {
                    continue;
                }

                float overshoot = factor - remaining;
                if (overshoot < lowestOvershoot)
                {
                    lowestOvershoot = overshoot;
                    bestCandidate = data;
                }
            }

            composition.Add(bestCandidate);
        }

        return composition.ToArray();
    }

    /// <summary>
    /// Create an enemy from pool/instantiate fallback using default data.
    /// </summary>
    public Enemy CreateEnemy(Vector3 position, Quaternion rotation)
    {
        EnemyData enemyData = GetFirstValidEnemyData();
        if (enemyData == null)
        {
            Debug.LogError("DifficultyScaler: No valid enemy data found in availableEnemyData.");
            return null;
        }

        return CreateEnemy(enemyData, position, rotation, 1, 1f, 1f);
    }

    /// <summary>
    /// Create an enemy from pool/instantiate fallback using specific data.
    /// </summary>
    public Enemy CreateEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        return CreateEnemy(enemyData, position, rotation, 1, 1f, 1f);
    }

    /// <summary>
    /// Create an enemy with wave-based scaling and additional multipliers.
    /// </summary>
    public Enemy CreateEnemy(EnemyData enemyData, Vector3 position, Quaternion rotation,
        int waveNumber, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        if (enemyData == null)
        {
            return null;
        }

        if (enemyData.Prefab == null)
        {
            Debug.LogError("[DifficultyScaler] enemyData.Prefab is NULL!");
            return null;
        }

        Enemy enemy = null;
        Enemy prefabComponent = enemyData.Prefab.GetComponent<Enemy>();

        if (prefabComponent != null)
        {
            enemy = PoolManager.Instance.Get(prefabComponent, position, rotation);
        }
        else
        {
            Debug.LogWarning("DifficultyScaler: enemyData.Prefab does not have an Enemy component.");
        }

        if (enemy == null)
        {
            GameObject enemyObject = Instantiate(enemyData.Prefab, position, rotation);
            enemy = enemyObject.GetComponent<Enemy>();
        }

        if (enemy == null)
        {
            Debug.LogWarning("DifficultyScaler: Failed to create enemy. Check pool configuration or prefab.");
            return null;
        }

        float finalHealthMultiplier = healthMultiplier * GetHealthMultiplier(waveNumber);
        float finalSpeedMultiplier = speedMultiplier * GetSpeedMultiplier(waveNumber);

        enemy.Initialize(enemyData, finalHealthMultiplier, finalSpeedMultiplier);
        return enemy;
    }

    /// <summary>
    /// Create an array of enemies from an explicit enemy type sequence.
    /// </summary>
    public Enemy[] CreateEnemies(EnemyData[] enemyTypes, Vector3 spawnCenter,
        float spawnRadius, int waveNumber, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            return Array.Empty<Enemy>();
        }

        Enemy[] enemies = new Enemy[enemyTypes.Length];

        for (int i = 0; i < enemyTypes.Length; i++)
        {
            EnemyData data = enemyTypes[i];
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = spawnCenter + new Vector3(randomOffset.x, randomOffset.y, 0f);

            enemies[i] = CreateEnemy(data, spawnPosition, Quaternion.identity, waveNumber, healthMultiplier, speedMultiplier);
        }

        return enemies;
    }

    /// <summary>
    /// Create multiple copies of one enemy type at random positions.
    /// </summary>
    public Enemy[] CreateEnemies(EnemyData enemyData, int count, Vector3 spawnCenter,
        float spawnRadius, int waveNumber)
    {
        if (count <= 0)
        {
            return Array.Empty<Enemy>();
        }

        Enemy[] enemies = new Enemy[count];

        for (int i = 0; i < count; i++)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPosition = spawnCenter + new Vector3(randomOffset.x, randomOffset.y, 0f);
            enemies[i] = CreateEnemy(enemyData, spawnPosition, Quaternion.identity, waveNumber);
        }

        return enemies;
    }

    /// <summary>
    /// Create enemies in a circle formation.
    /// </summary>
    public Enemy[] CreateEnemiesInCircle(EnemyData enemyData, int count, Vector3 center,
        float radius, int waveNumber)
    {
        if (count <= 0)
        {
            return Array.Empty<Enemy>();
        }

        Enemy[] enemies = new Enemy[count];
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            Vector3 spawnPosition = new Vector3(x, y, 0f);

            Vector2 directionToCenter = ((Vector2)center - new Vector2(x, y)).normalized;
            float rotation = Mathf.Atan2(directionToCenter.y, directionToCenter.x) * Mathf.Rad2Deg - 90f;
            Quaternion spawnRotation = Quaternion.Euler(0f, 0f, rotation);

            enemies[i] = CreateEnemy(enemyData, spawnPosition, spawnRotation, waveNumber);
        }

        return enemies;
    }

    /// <summary>
    /// Spawn enemies for a wave target based on difficulty factors.
    /// </summary>
    public Enemy[] CreateEnemiesByDifficulty(float targetDifficulty, Vector3 spawnCenter,
        float spawnRadius, int waveNumber, float healthMultiplier = 1f, float speedMultiplier = 1f)
    {
        EnemyData[] composition = CalculateEnemyComposition(targetDifficulty);
        Enemy[] enemies = CreateEnemies(composition, spawnCenter, spawnRadius, waveNumber, healthMultiplier, speedMultiplier);

        float actualDifficulty = 0f;
        for (int i = 0; i < composition.Length; i++)
        {
            if (composition[i] != null)
            {
                actualDifficulty += Mathf.Max(0.01f, composition[i].DifficultyFactor);
            }
        }

        lastWaveDifficulty = actualDifficulty;
        return enemies;
    }

    /// <summary>
    /// Calculate difficulty rating for UI display.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Difficulty rating (0-1 normalized).</returns>
    public float GetDifficultyRating(int waveNumber)
    {
        float healthNorm = Normalize(GetHealthMultiplier(waveNumber), baseHealthMultiplier, maxHealthMultiplier);
        float speedNorm = Normalize(GetSpeedMultiplier(waveNumber), baseSpeedMultiplier, maxSpeedMultiplier);
        float damageNorm = Normalize(GetDamageMultiplier(waveNumber), baseDamageMultiplier, maxDamageMultiplier);
        float countNorm = Normalize(GetEnemyCount(waveNumber), baseEnemyCount, maxEnemyCount);

        // Average of all factors
        float rating = (healthNorm + speedNorm + damageNorm + countNorm) / 4f;
        return Mathf.Clamp01(rating);
    }

    private static float Normalize(float value, float min, float max)
    {
        float denominator = max - min;
        if (Mathf.Abs(denominator) < 0.0001f)
        {
            return 0f;
        }

        return (value - min) / denominator;
    }

    private List<EnemyData> GetValidEnemyPool()
    {
        List<EnemyData> pool = new List<EnemyData>();

        if (enemyData != null)
        {
            for (int i = 0; i < enemyData.Length; i++)
            {
                EnemyData data = enemyData[i];
                if (data != null && data.Prefab != null)
                {
                    pool.Add(data);
                }
            }
        }

        return pool;
    }

    private EnemyData GetFirstValidEnemyData()
    {
        if (enemyData == null)
        {
            return null;
        }

        for (int i = 0; i < enemyData.Length; i++)
        {
            EnemyData data = enemyData[i];
            if (data != null && data.Prefab != null)
            {
                return data;
            }
        }

        return null;
    }

    /// <summary>
    /// Reset scaling to default values.
    /// </summary>
    public void ResetToDefaults()
    {
        difficultyMultiplier = 1f;
        lastWaveDifficulty = 0f;

        baseHealthMultiplier = 1f;
        healthIncreasePerWave = 0.1f;
        maxHealthMultiplier = 5f;

        baseSpeedMultiplier = 1f;
        speedIncreasePerWave = 0.05f;
        maxSpeedMultiplier = 2f;

        baseDamageMultiplier = 1f;
        damageIncreasePerWave = 0.1f;
        maxDamageMultiplier = 3f;

        baseEnemyCount = 5;
        enemyIncreasePerWave = 2;
        maxEnemyCount = 50;

        baseSpawnInterval = 0.5f;
        spawnIntervalDecreasePerWave = 0.02f;
        minSpawnInterval = 0.1f;
    }
}

/// <summary>
/// Struct containing all difficulty values for a wave.
/// </summary>
[Serializable]
public struct WaveDifficultyValues
{
    public float healthMultiplier;
    public float speedMultiplier;
    public float damageMultiplier;
    public int enemyCount;
    public float spawnInterval;
}
