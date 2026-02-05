using System;
using UnityEngine;

/// <summary>
/// Calculates per-wave scaling for enemy stats.
/// Provides multipliers for health, speed, damage, and enemy count.
/// </summary>
[Serializable]
public class DifficultyScaler : MonoBehaviour
{
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
    /// Calculate difficulty rating for UI display.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Difficulty rating (0-1 normalized).</returns>
    public float GetDifficultyRating(int waveNumber)
    {
        // Calculate normalized values
        float healthNorm = (GetHealthMultiplier(waveNumber) - baseHealthMultiplier) /
                           (maxHealthMultiplier - baseHealthMultiplier);
        float speedNorm = (GetSpeedMultiplier(waveNumber) - baseSpeedMultiplier) /
                          (maxSpeedMultiplier - baseSpeedMultiplier);
        float damageNorm = (GetDamageMultiplier(waveNumber) - baseDamageMultiplier) /
                           (maxDamageMultiplier - baseDamageMultiplier);
        float countNorm = (float)(GetEnemyCount(waveNumber) - baseEnemyCount) /
                          (maxEnemyCount - baseEnemyCount);

        // Average of all factors
        float rating = (healthNorm + speedNorm + damageNorm + countNorm) / 4f;
        return Mathf.Clamp01(rating);
    }

    /// <summary>
    /// Reset scaling to default values.
    /// </summary>
    public void ResetToDefaults()
    {
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
