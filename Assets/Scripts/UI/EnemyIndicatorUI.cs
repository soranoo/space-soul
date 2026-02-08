using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages poolable off-screen enemy indicator arrows.
/// Listens to WaveManager enemy spawn/die events and allocates indicators.
/// </summary>
public class EnemyIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private OffScreenIndicator indicatorPrefab;
    [SerializeField] private RectTransform canvasRect;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 10;

    [Header("Display")]
    [Tooltip("Padding in pixels from the screen edge.")]
    [SerializeField] private float edgePadding = 40f;

    private Camera mainCamera;
    private WaveManager waveManager;
    private ObjectPool<OffScreenIndicator> pool;

    private readonly Dictionary<Enemy, OffScreenIndicator> activeIndicators = new Dictionary<Enemy, OffScreenIndicator>();

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

        pool = new ObjectPool<OffScreenIndicator>(indicatorPrefab, initialPoolSize, poolParent, true);
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
        foreach (KeyValuePair<Enemy, OffScreenIndicator> kvp in activeIndicators)
        {
            kvp.Value.UpdateIndicator(mainCamera, canvasRect, edgePadding);
        }
    }

    private void OnEnemySpawned(Enemy enemy)
    {
        if (enemy == null || pool == null)
        {
            return;
        }

        if (activeIndicators.ContainsKey(enemy))
        {
            return;
        }

        OffScreenIndicator indicator = pool.Get();
        if (indicator == null)
        {
            return;
        }

        // Reparent into the canvas so the RectTransform renders correctly
        indicator.transform.SetParent(transform, false);
        indicator.SetTarget(enemy.transform);
        activeIndicators[enemy] = indicator;
    }

    private void OnEnemyDied(Enemy enemy)
    {
        if (enemy == null || pool == null)
        {
            return;
        }

        if (!activeIndicators.TryGetValue(enemy, out OffScreenIndicator indicator))
        {
            return;
        }

        pool.Release(indicator);
        activeIndicators.Remove(enemy);
    }

    private void ReleaseAll()
    {
        if (pool == null)
        {
            return;
        }

        foreach (KeyValuePair<Enemy, OffScreenIndicator> kvp in activeIndicators)
        {
            if (kvp.Value != null)
            {
                pool.Release(kvp.Value);
            }
        }

        activeIndicators.Clear();
    }
}
