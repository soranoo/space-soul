using System;
using UnityEngine;

/// <summary>
/// Defines a single enemy spawn entry within a wave.
/// </summary>
[Serializable]
public class EnemySpawnEntry
{
    [Tooltip("Enemy type data to spawn.")]
    public EnemyData enemyData;

    [Tooltip("Number of this enemy type to spawn.")]
    public int count = 1;

    [Tooltip("Delay before spawning this group (seconds).")]
    public float spawnDelay = 0f;
}

/// <summary>
/// Defines wave composition and difficulty.
/// Enables data-driven wave design.
/// </summary>
[CreateAssetMenu(fileName = "NewWaveConfig", menuName = "Game/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Wave Info")]
    [Tooltip("Display name for this wave.")]
    [SerializeField] private string waveName = "Wave";

    [Tooltip("Wave number this config is designed for.")]
    [SerializeField] private int waveNumber = 1;

    [Header("Enemy Spawns")]
    [Tooltip("List of enemy types and counts to spawn.")]
    [SerializeField] private EnemySpawnEntry[] enemySpawns;

    [Header("Spawn Timing")]
    [Tooltip("Time between individual enemy spawns (seconds).")]
    [SerializeField] private float spawnInterval = 0.5f;

    [Tooltip("Delay before wave starts (seconds).")]
    [SerializeField] private float waveStartDelay = 2f;

    [Header("Difficulty Multipliers")]
    [Tooltip("Health multiplier for this wave.")]
    [SerializeField] private float healthMultiplier = 1f;

    [Tooltip("Speed multiplier for this wave.")]
    [SerializeField] private float speedMultiplier = 1f;

    [Tooltip("Damage multiplier for this wave.")]
    [SerializeField] private float damageMultiplier = 1f;

    [Header("Spawn Pattern")]
    [Tooltip("Pattern for spawning enemies.")]
    [SerializeField] private SpawnPatternType spawnPattern = SpawnPatternType.Random;

    [Tooltip("Minimum distance from player to spawn.")]
    [SerializeField] private float minSpawnDistance = 8f;

    [Tooltip("Maximum distance from player to spawn.")]
    [SerializeField] private float maxSpawnDistance = 15f;

    /// <summary>
    /// Display name for this wave.
    /// </summary>
    public string WaveName => waveName;

    /// <summary>
    /// Wave number.
    /// </summary>
    public int WaveNumber => waveNumber;

    /// <summary>
    /// Enemy spawn entries.
    /// </summary>
    public EnemySpawnEntry[] EnemySpawns => enemySpawns;

    /// <summary>
    /// Time between spawns.
    /// </summary>
    public float SpawnInterval => spawnInterval;

    /// <summary>
    /// Delay before wave starts.
    /// </summary>
    public float WaveStartDelay => waveStartDelay;

    /// <summary>
    /// Health multiplier.
    /// </summary>
    public float HealthMultiplier => healthMultiplier;

    /// <summary>
    /// Speed multiplier.
    /// </summary>
    public float SpeedMultiplier => speedMultiplier;

    /// <summary>
    /// Damage multiplier.
    /// </summary>
    public float DamageMultiplier => damageMultiplier;

    /// <summary>
    /// Spawn pattern type.
    /// </summary>
    public SpawnPatternType SpawnPattern => spawnPattern;

    /// <summary>
    /// Minimum spawn distance from player.
    /// </summary>
    public float MinSpawnDistance => minSpawnDistance;

    /// <summary>
    /// Maximum spawn distance from player.
    /// </summary>
    public float MaxSpawnDistance => maxSpawnDistance;

    /// <summary>
    /// Get total enemy count for this wave.
    /// </summary>
    /// <returns>Total number of enemies.</returns>
    public int GetTotalEnemyCount()
    {
        int total = 0;
        if (enemySpawns != null)
        {
            for (int i = 0; i < enemySpawns.Length; i++)
            {
                total += enemySpawns[i].count;
            }
        }
        return total;
    }
}

/// <summary>
/// Types of spawn patterns.
/// </summary>
public enum SpawnPatternType
{
    Random,
    Circle,
    Edges,
    Corners
}
