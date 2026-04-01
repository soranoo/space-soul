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
    [Tooltip("Use procedural generation after configured waves.")]
    [SerializeField] private bool generateWavesAfterConfigs = true;

    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private DifficultyScaler difficultyScaler;

    [Header("Power-Ups")]
    [SerializeField] private PowerUpPickup[] powerUpPickupPrefabs;

    // "Auto-loaded from Resources/Waves. Sorted by wave number, only enabled waves are used."
    private WaveConfig[] waveConfigs;
    private int currentWaveNumber;
    private int enemiesAlive;
    private int enemiesSpawned;
    private int totalEnemiesToSpawn;
    private float currentWaveDifficulty;
    private bool isWaveActive;
    private bool isSpawning;
    private Coroutine spawnCoroutine;
    private EnemyData[] proceduralEnemyComposition;
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

    /// <summary>
    /// Event fired when an enemy that has ShowAlarmOnSpawn enabled is spawned.
    /// </summary>
    public event Action<Enemy> AlarmEnemySpawned;

    protected override void Awake()
    {
        base.Awake();

        LoadWaveConfigsFromResources();

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
    /// Load all enabled WaveConfig assets from Resources/Waves, sorted by wave number.
    /// </summary>
    private void LoadWaveConfigsFromResources()
    {
        WaveConfig[] allConfigs = Resources.LoadAll<WaveConfig>("Waves");

        List<WaveConfig> enabledConfigs = new List<WaveConfig>();
        for (int i = 0; i < allConfigs.Length; i++)
        {
            if (allConfigs[i].WaveEnabled)
            {
                enabledConfigs.Add(allConfigs[i]);
            }
        }

        enabledConfigs.Sort((a, b) => a.WaveNumber.CompareTo(b.WaveNumber));
        waveConfigs = enabledConfigs.ToArray();
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
        currentWaveDifficulty = 0f;
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
        if (difficultyScaler == null)
        {
            Debug.LogError("WaveManager: DifficultyScaler is required for procedural waves.");
            return;
        }

        isWaveActive = true;
        enemiesAlive = 0;
        enemiesSpawned = 0;
        currentWaveDifficulty = 0f;

        proceduralEnemyComposition = Array.Empty<EnemyData>();

        float targetDifficulty = difficultyScaler.GetTargetWaveDifficulty(currentWaveNumber);
        proceduralEnemyComposition = difficultyScaler.CalculateEnemyComposition(targetDifficulty);

        if (proceduralEnemyComposition.Length == 0)
        {
            Debug.LogError("WaveManager: Procedural composition is empty. Configure availableEnemyData in DifficultyScaler.");
            isWaveActive = false;
            return;
        }

        totalEnemiesToSpawn = proceduralEnemyComposition.Length;

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
        if (spawns != null)
        {
            for (int i = 0; i < spawns.Length; i++)
            {
                EnemySpawnEntry entry = spawns[i];

                if (entry == null || entry.enemyData == null || entry.count <= 0)
                {
                    continue;
                }

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
        }

        isSpawning = false;
        if (isWaveActive && enemiesAlive <= 0)
        {
            CompleteWave();
        }
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

        if (proceduralEnemyComposition == null || proceduralEnemyComposition.Length == 0)
        {
            isSpawning = false;
            if (isWaveActive && enemiesAlive <= 0)
            {
                CompleteWave();
            }
            yield break;
        }

        for (int i = 0; i < proceduralEnemyComposition.Length; i++)
        {
            SpawnEnemy(proceduralEnemyComposition[i], null, healthMult, speedMult);
            yield return new WaitForSeconds(0.5f);
        }

        isSpawning = false;
        if (isWaveActive && enemiesAlive <= 0)
        {
            CompleteWave();
        }
    }

    /// <summary>
    /// Spawn a single enemy.
    /// </summary>
    private void SpawnEnemy(EnemyData enemyData, WaveConfig config, float healthMult, float speedMult)
    {
        if (enemyData == null)
        {
            return;
        }

        Vector3 spawnPosition;

        if (spawnManager != null && config != null)
        {
            spawnPosition = spawnManager.GetSpawnPosition(config.SpawnPattern);
        }
        else if (spawnManager != null)
        {
            spawnPosition = spawnManager.GetSpawnPosition(SpawnPatternType.Random);
        }
        else
        {
            spawnPosition = GetDefaultSpawnPosition();
        }

        Enemy enemy = null;

        if (difficultyScaler != null)
        {
            enemy = difficultyScaler.CreateEnemy(
                enemyData,
                spawnPosition,
                Quaternion.identity,
                currentWaveNumber,
                healthMult,
                speedMult
            );
        }
        else if (enemyData.Prefab != null)
        {
            GameObject enemyObject = Instantiate(enemyData.Prefab, spawnPosition, Quaternion.identity);
            enemy = enemyObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.Initialize(enemyData, healthMult, speedMult);
            }
        }

        if (enemy != null)
        {
            enemiesSpawned++;
            enemiesAlive++;
            currentWaveDifficulty += Mathf.Max(0.01f, enemyData.DifficultyFactor);
            enemy.Died += OnEnemyDied;

            // Subscribe to spawner enemies' child spawn requests
            if (enemyData.CanSpawnEnemies)
            {
                enemy.SpawnSoldiersRequested += OnSpawnerEnemySpawnChildren;
            }

            EnemySpawned?.Invoke(enemy);

            if (enemyData.ShowAlarmOnSpawn)
            {
                AlarmEnemySpawned?.Invoke(enemy);
            }
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

            if (difficultyScaler != null)
            {
                soldier = difficultyScaler.CreateEnemy(
                    childData,
                    spawnPosition,
                    Quaternion.identity,
                    currentWaveNumber,
                    1f,
                    1f
                );
            }
            else if (childData.Prefab != null)
            {
                GameObject enemyObject = Instantiate(childData.Prefab, spawnPosition, Quaternion.identity);
                soldier = enemyObject.GetComponent<Enemy>();

                if (soldier != null)
                {
                    soldier.Initialize(childData, 1f, 1f);
                }
            }

            if (soldier != null)
            {
                enemiesAlive++;
                currentWaveDifficulty += Mathf.Max(0.01f, childData.DifficultyFactor);
                soldier.Died += OnEnemyDied;

                if (spawner != null)
                {
                    childToSpawnerMap[soldier] = spawner;
                    spawner.NotifyChildSpawned();
                }

                EnemySpawned?.Invoke(soldier);

                if (childData.ShowAlarmOnSpawn)
                {
                    AlarmEnemySpawned?.Invoke(soldier);
                }
            }
        }
    }

    /// <summary>
    /// Get a default spawn position when SpawnManager is not available.
    /// Falls back to a point just outside a rough 1.5x viewport estimate.
    /// </summary>
    private Vector3 GetDefaultSpawnPosition()
    {
        Camera cam = Camera.main;
        float halfH = cam != null ? cam.orthographicSize * 1.5f : 10f;
        float halfW = cam != null ? halfH * cam.aspect : 15f;

        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        float tX = cos != 0f ? halfW / Mathf.Abs(cos) : float.MaxValue;
        float tY = sin != 0f ? halfH / Mathf.Abs(sin) : float.MaxValue;
        float t = Mathf.Min(tX, tY);

        Vector3 center = cam != null ? cam.transform.position : Vector3.zero;
        center.z = 0f;
        return center + new Vector3(cos * t, sin * t, 0f);
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
        if (difficultyScaler != null)
        {
            difficultyScaler.SetLastWaveDifficulty(currentWaveDifficulty);
        }
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
        currentWaveDifficulty = 0f;
        proceduralEnemyComposition = Array.Empty<EnemyData>();
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
