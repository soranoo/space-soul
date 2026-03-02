using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls wave progression and enemy spawning.
/// Implements Singleton pattern.
/// </summary>
public class WaveManager : SingletonBase<WaveManager>
{
    [Header("Wave Configuration")]
    [Tooltip("Array of wave configurations in order.")]
    [SerializeField] private WaveConfig[] waveConfigs;

    [Tooltip("Use procedural generation after configured waves.")]
    [SerializeField] private bool generateWavesAfterConfigs = true;

    [Tooltip("Default enemy data for procedural waves.")]
    [SerializeField] private EnemyData defaultEnemyData;

    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private DifficultyScaler difficultyScaler;

    [Header("Power-Ups")]
    [SerializeField] private PowerUpPickup[] powerUpPickupPrefabs;

    private int currentWaveNumber;
    private int enemiesAlive;
    private int enemiesSpawned;
    private int totalEnemiesToSpawn;
    private bool isWaveActive;
    private bool isSpawning;
    private Coroutine spawnCoroutine;
    private readonly Dictionary<Enemy, Enemy> childToSpawnerMap = new Dictionary<Enemy, Enemy>();

    /// <summary>
    /// Current wave number (1-based).
    /// </summary>
    public int CurrentWaveNumber => currentWaveNumber;

    /// <summary>
    /// Number of enemies currently alive.
    /// </summary>
    public int EnemiesAlive => enemiesAlive;

    /// <summary>
    /// Whether a wave is currently active.
    /// </summary>
    public bool IsWaveActive => isWaveActive;

    /// <summary>
    /// Event fired when a wave starts.
    /// </summary>
    public event Action<int> WaveStarted;

    /// <summary>
    /// Event fired when a wave is completed.
    /// </summary>
    public event Action<int> WaveCompleted;

    /// <summary>
    /// Event fired when an enemy is spawned.
    /// </summary>
    public event Action<Enemy> EnemySpawned;

    /// <summary>
    /// Event fired when an enemy dies.
    /// </summary>
    public event Action<Enemy> EnemyDied;

    /// <summary>
    /// Event fired when all configured waves are complete.
    /// </summary>
    public event Action AllWavesCompleted;

    protected override void Awake()
    {
        base.Awake();

        if (spawnManager == null)
        {
            spawnManager = GetComponent<SpawnManager>();
        }

        if (difficultyScaler == null)
        {
            difficultyScaler = GetComponent<DifficultyScaler>();
        }
    }

    /// <summary>
    /// Start the wave system from wave 1.
    /// </summary>
    public void StartWaves()
    {
        currentWaveNumber = 0;
        childToSpawnerMap.Clear();
        StartNextWave();
    }

    /// <summary>
    /// Start the next wave.
    /// </summary>
    public void StartNextWave()
    {
        if (isWaveActive)
        {
            Debug.LogWarning("WaveManager: Cannot start next wave while current wave is active.");
            return;
        }

        currentWaveNumber++;

        WaveConfig config = GetWaveConfig(currentWaveNumber);
        if (config != null)
        {
            StartWave(config);
        }
        else if (generateWavesAfterConfigs)
        {
            StartProceduralWave();
        }
        else
        {
            AllWavesCompleted?.Invoke();
        }
    }

    /// <summary>
    /// Get wave config for a specific wave number.
    /// </summary>
    /// <param name="waveNumber">Wave number (1-based).</param>
    /// <returns>Wave config or null.</returns>
    private WaveConfig GetWaveConfig(int waveNumber)
    {
        if (waveConfigs == null || waveConfigs.Length == 0)
        {
            return null;
        }

        int index = waveNumber - 1;
        if (index >= 0 && index < waveConfigs.Length)
        {
            return waveConfigs[index];
        }

        return null;
    }

    /// <summary>
    /// Start a wave with the given configuration.
    /// </summary>
    /// <param name="config">Wave configuration.</param>
    private void StartWave(WaveConfig config)
    {
        isWaveActive = true;
        enemiesAlive = 0;
        enemiesSpawned = 0;
        totalEnemiesToSpawn = config.GetTotalEnemyCount();

        WaveStarted?.Invoke(currentWaveNumber);

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnWaveEnemies(config));
    }

    /// <summary>
    /// Start a procedurally generated wave.
    /// </summary>
    private void StartProceduralWave()
    {
        if (defaultEnemyData == null)
        {
            Debug.LogError("WaveManager: No default enemy data for procedural waves.");
            return;
        }

        isWaveActive = true;
        enemiesAlive = 0;
        enemiesSpawned = 0;

        // Calculate procedural enemy count based on wave number
        int baseCount = 5;
        int additionalPerWave = 2;
        totalEnemiesToSpawn = baseCount + (currentWaveNumber - 1) * additionalPerWave;

        WaveStarted?.Invoke(currentWaveNumber);

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnProceduralWave());
    }

    /// <summary>
    /// Coroutine to spawn enemies for a configured wave.
    /// </summary>
    private IEnumerator SpawnWaveEnemies(WaveConfig config)
    {
        isSpawning = true;

        // Initial delay
        yield return new WaitForSeconds(config.WaveStartDelay);

        // Get difficulty multipliers
        float healthMult = config.HealthMultiplier;
        float speedMult = config.SpeedMultiplier;

        if (difficultyScaler != null)
        {
            healthMult *= difficultyScaler.GetHealthMultiplier(currentWaveNumber);
            speedMult *= difficultyScaler.GetSpeedMultiplier(currentWaveNumber);
        }

        // Spawn each enemy group
        EnemySpawnEntry[] spawns = config.EnemySpawns;
        for (int i = 0; i < spawns.Length; i++)
        {
            EnemySpawnEntry entry = spawns[i];

            // Delay for this group
            if (entry.spawnDelay > 0f)
            {
                yield return new WaitForSeconds(entry.spawnDelay);
            }

            // Spawn enemies in this group
            for (int j = 0; j < entry.count; j++)
            {
                SpawnEnemy(entry.enemyData, config, healthMult, speedMult);
                yield return new WaitForSeconds(config.SpawnInterval);
            }
        }

        isSpawning = false;
    }

    /// <summary>
    /// Coroutine to spawn procedural wave enemies.
    /// </summary>
    private IEnumerator SpawnProceduralWave()
    {
        isSpawning = true;

        // Initial delay
        yield return new WaitForSeconds(2f);

        // Get difficulty multipliers
        float healthMult = 1f;
        float speedMult = 1f;

        if (difficultyScaler != null)
        {
            healthMult = difficultyScaler.GetHealthMultiplier(currentWaveNumber);
            speedMult = difficultyScaler.GetSpeedMultiplier(currentWaveNumber);
        }

        // Spawn all enemies
        for (int i = 0; i < totalEnemiesToSpawn; i++)
        {
            SpawnEnemy(defaultEnemyData, null, healthMult, speedMult);
            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false;
    }

    /// <summary>
    /// Spawn a single enemy.
    /// </summary>
    private void SpawnEnemy(EnemyData enemyData, WaveConfig config, float healthMult, float speedMult)
    {
        Vector3 spawnPosition;

        if (spawnManager != null && config != null)
        {
            spawnPosition = spawnManager.GetSpawnPosition(
                config.SpawnPattern,
                config.MinSpawnDistance,
                config.MaxSpawnDistance
            );
        }
        else if (spawnManager != null)
        {
            spawnPosition = spawnManager.GetSpawnPosition(SpawnPatternType.Random, 8f, 15f);
        }
        else
        {
            spawnPosition = GetDefaultSpawnPosition();
        }

        Enemy enemy = null;

        if (EnemyFactory.Instance != null)
        {
            enemy = EnemyFactory.Instance.CreateEnemy(
                enemyData,
                spawnPosition,
                Quaternion.identity,
                currentWaveNumber,
                healthMult,
                speedMult
            );
        }

        if (enemy != null)
        {
            enemiesSpawned++;
            enemiesAlive++;
            enemy.Died += OnEnemyDied;

            // Subscribe to spawner enemies' child spawn requests
            if (enemyData.CanSpawnEnemies)
            {
                enemy.SpawnSoldiersRequested += OnSpawnerEnemySpawnChildren;
            }

            EnemySpawned?.Invoke(enemy);
        }
    }

    /// <summary>
    /// Handle spawner enemy requesting child spawns.
    /// </summary>
    private void OnSpawnerEnemySpawnChildren(Enemy spawner, EnemyData childData, int count)
    {
        if (childData == null || count <= 0)
        {
            return;
        }

        // Spawn children around the spawner enemy
        for (int i = 0; i < count; i++)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-2f, 2f),
                UnityEngine.Random.Range(-2f, 2f),
                0f
            );
            Vector3 spawnPosition = spawner.transform.position + offset;

            Enemy soldier = null;

            if (EnemyFactory.Instance != null)
            {
                soldier = EnemyFactory.Instance.CreateEnemy(
                    childData,
                    spawnPosition,
                    Quaternion.identity,
                    currentWaveNumber,
                    1f,
                    1f
                );
            }

            if (soldier != null)
            {
                enemiesAlive++;
                soldier.Died += OnEnemyDied;

                if (spawner != null)
                {
                    childToSpawnerMap[soldier] = spawner;
                    spawner.NotifyChildSpawned();
                }

                EnemySpawned?.Invoke(soldier);
            }
        }
    }

    /// <summary>
    /// Get a default spawn position when SpawnManager is not available.
    /// </summary>
    private Vector3 GetDefaultSpawnPosition()
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = UnityEngine.Random.Range(10f, 15f);
        return new Vector3(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance, 0f);
    }

    /// <summary>
    /// Called when an enemy dies.
    /// </summary>
    private void OnEnemyDied(Enemy enemy)
    {
        if (enemy != null && childToSpawnerMap.TryGetValue(enemy, out Enemy spawner))
        {
            childToSpawnerMap.Remove(enemy);
            if (spawner != null)
            {
                spawner.NotifyChildDespawned();
            }
        }

        if (enemy != null)
        {
            RemoveTrackedChildrenForSpawner(enemy);
        }

        enemy.Died -= OnEnemyDied;
        enemy.SpawnSoldiersRequested -= OnSpawnerEnemySpawnChildren;
        enemiesAlive--;
        EnemyDied?.Invoke(enemy);

        if (enemy != null && enemy.Data != null && !enemy.Data.SelfDestructOnContact)
        {
            ScoreManager.Instance?.AddScore(enemy.Data.ScoreValue);
            TrySpawnPowerUp(enemy);
        }

        // Check for wave completion
        if (!isSpawning && enemiesAlive <= 0 && isWaveActive)
        {
            CompleteWave();
        }
    }

    private void RemoveTrackedChildrenForSpawner(Enemy spawner)
    {
        if (spawner == null || childToSpawnerMap.Count == 0)
        {
            return;
        }

        List<Enemy> childrenToForget = null;

        foreach (KeyValuePair<Enemy, Enemy> pair in childToSpawnerMap)
        {
            if (pair.Value == spawner)
            {
                if (childrenToForget == null)
                {
                    childrenToForget = new List<Enemy>();
                }

                childrenToForget.Add(pair.Key);
            }
        }

        if (childrenToForget == null)
        {
            return;
        }

        for (int i = 0; i < childrenToForget.Count; i++)
        {
            childToSpawnerMap.Remove(childrenToForget[i]);
        }
    }

    private void TrySpawnPowerUp(Enemy enemy)
    {
        if (powerUpPickupPrefabs == null || powerUpPickupPrefabs.Length == 0)
        {
            return;
        }

        if (enemy == null || enemy.Data == null)
        {
            return;
        }

        float dropChance = Mathf.Clamp01(enemy.Data.PowerUpDropChance);
        if (dropChance <= 0f || UnityEngine.Random.value > dropChance)
        {
            return;
        }

        PowerUpPickup selectedPrefab = powerUpPickupPrefabs[UnityEngine.Random.Range(0, powerUpPickupPrefabs.Length)];
        if (selectedPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = enemy.transform.position;

        if (selectedPrefab != null)
        {
            PowerUpPickup pickup = null;

            PowerUpPickup prefabComponent = selectedPrefab.GetComponent<PowerUpPickup>();
            if (prefabComponent != null)
            {
                pickup = PoolManager.Instance.Get(prefabComponent, spawnPosition, Quaternion.identity);
            }

            if (pickup == null)
            {
                pickup = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    /// <summary>
    /// Complete the current wave.
    /// </summary>
    private void CompleteWave()
    {
        isWaveActive = false;
        WaveCompleted?.Invoke(currentWaveNumber);
    }

    /// <summary>
    /// Force complete the current wave (for debugging).
    /// </summary>
    public void ForceCompleteWave()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        isSpawning = false;

        // Kill all remaining enemies
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].TakeDamage(9999);
        }
    }

    /// <summary>
    /// Reset the wave system.
    /// </summary>
    public void ResetWaves()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        isWaveActive = false;
        isSpawning = false;
        currentWaveNumber = 0;
        enemiesAlive = 0;
        enemiesSpawned = 0;
        totalEnemiesToSpawn = 0;
    }

    /// <summary>
    /// Get progress of current wave (0-1).
    /// </summary>
    /// <returns>Progress percentage.</returns>
    public float GetWaveProgress()
    {
        if (totalEnemiesToSpawn <= 0)
        {
            return 0f;
        }

        int enemiesDefeated = enemiesSpawned - enemiesAlive;
        return (float)enemiesDefeated / totalEnemiesToSpawn;
    }
}
