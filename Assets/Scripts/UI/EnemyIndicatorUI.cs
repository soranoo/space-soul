using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages poolable off-screen enemy indicator arrows.
/// Listens to WaveManager enemy spawn/die events and allocates indicators.
/// </summary>
public class EnemyIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyOffScreenIndicator indicatorPrefab;
    [SerializeField] private RectTransform canvasRect;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    [Header("Display")]
    [Tooltip("Padding in pixels from the screen edge.")]
    [SerializeField] private float edgePadding = 40f;

    private Camera mainCamera;
    private WaveManager waveManager;
    private ObjectPool<EnemyOffScreenIndicator> pool;

    private readonly Dictionary<Enemy, EnemyOffScreenIndicator> activeIndicators = new Dictionary<Enemy, EnemyOffScreenIndicator>();
    private readonly HashSet<Enemy> trackedEnemies = new HashSet<Enemy>();
    private readonly List<Enemy> removeList = new List<Enemy>();

    private void Awake()
    {
        mainCamera = Camera.main;

        if (canvasRect == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
            }
        }
    }

    private void OnEnable()
    {
        EnsurePool();

        waveManager = WaveManager.Instance;
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        ReleaseAll();
    }

    private void Subscribe()
    {
        if (waveManager == null)
        {
            return;
        }

        waveManager.EnemySpawned += OnEnemySpawned;
        waveManager.EnemyDied += OnEnemyDied;
    }

    private void Unsubscribe()
    {
        if (waveManager == null)
        {
            return;
        }

        waveManager.EnemySpawned -= OnEnemySpawned;
        waveManager.EnemyDied -= OnEnemyDied;
    }

    private void EnsurePool()
    {
        if (pool != null || indicatorPrefab == null)
        {
            return;
        }

        Transform poolParent = new GameObject("IndicatorPool").transform;
        poolParent.SetParent(transform);

        pool = new ObjectPool<EnemyOffScreenIndicator>(indicatorPrefab, initialPoolSize, poolParent, true);
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }
        }

        // Iterate and update every active indicator
        removeList.Clear();

        foreach (Enemy enemy in trackedEnemies)
        {
            if (enemy == null)
            {
                removeList.Add(enemy);
                continue;
            }

            bool onScreen = OffScreenIndicatorBase.IsOnScreen(mainCamera, enemy.transform.position);

            if (onScreen)
            {
                if (activeIndicators.TryGetValue(enemy, out EnemyOffScreenIndicator existing))
                {
                    pool.Release(existing);
                    activeIndicators.Remove(enemy);
                }

                continue;
            }

            if (!activeIndicators.TryGetValue(enemy, out EnemyOffScreenIndicator indicator))
            {
                indicator = pool.Get();
                if (indicator == null)
                {
                    continue;
                }

                indicator.transform.SetParent(transform, false);
                indicator.SetTarget(enemy.transform);
                activeIndicators[enemy] = indicator;
            }

            indicator.UpdateIndicator(mainCamera, canvasRect, edgePadding);
        }

        for (int i = 0; i < removeList.Count; i++)
        {
            Enemy enemy = removeList[i];
            trackedEnemies.Remove(enemy);

            if (activeIndicators.TryGetValue(enemy, out EnemyOffScreenIndicator indicator))
            {
                pool.Release(indicator);
                activeIndicators.Remove(enemy);
            }
        }
    }

    private void OnEnemySpawned(Enemy enemy)
    {
        if (enemy == null || pool == null)
        {
            return;
        }

        trackedEnemies.Add(enemy);
    }

    private void OnEnemyDied(Enemy enemy)
    {
        if (enemy == null || pool == null)
        {
            return;
        }

        trackedEnemies.Remove(enemy);

        if (activeIndicators.TryGetValue(enemy, out EnemyOffScreenIndicator indicator))
        {
            pool.Release(indicator);
            activeIndicators.Remove(enemy);
        }
    }

    private void ReleaseAll()
    {
        if (pool == null)
        {
            return;
        }

        foreach (KeyValuePair<Enemy, EnemyOffScreenIndicator> kvp in activeIndicators)
        {
            if (kvp.Value != null)
            {
                pool.Release(kvp.Value);
            }
        }

        activeIndicators.Clear();
    }
}
