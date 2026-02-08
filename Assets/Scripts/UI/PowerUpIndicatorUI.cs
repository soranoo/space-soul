using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages poolable off-screen power-up indicators.
/// Scans for active PowerUpPickup instances each frame and allocates/releases indicators.
/// </summary>
public class PowerUpIndicatorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PowerUpOffScreenIndicator indicatorPrefab;
    [SerializeField] private RectTransform canvasRect;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 5;

    [Header("Display")]
    [Tooltip("Padding in pixels from the screen edge.")]
    [SerializeField] private float edgePadding = 50f;

    private Camera mainCamera;
    private ObjectPool<PowerUpOffScreenIndicator> pool;

    private readonly Dictionary<PowerUpPickup, PowerUpOffScreenIndicator> activeIndicators =
        new Dictionary<PowerUpPickup, PowerUpOffScreenIndicator>();

    private readonly List<PowerUpPickup> removeList = new List<PowerUpPickup>();

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
    }

    private void OnDisable()
    {
        ReleaseAll();
    }

    private void EnsurePool()
    {
        if (pool != null || indicatorPrefab == null)
        {
            return;
        }

        Transform poolParent = new GameObject("PowerUpIndicatorPool").transform;
        poolParent.SetParent(transform);

        pool = new ObjectPool<PowerUpOffScreenIndicator>(indicatorPrefab, initialPoolSize, poolParent, true);
    }

    private void LateUpdate()
    {
        if (pool == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        // Find all active pickups in the scene
        PowerUpPickup[] pickups = FindObjectsByType<PowerUpPickup>(FindObjectsSortMode.None);

        // Track which pickups are still alive this frame
        HashSet<PowerUpPickup> aliveSet = new HashSet<PowerUpPickup>();

        for (int i = 0; i < pickups.Length; i++)
        {
            PowerUpPickup pickup = pickups[i];
            if (pickup == null || !pickup.gameObject.activeInHierarchy)
            {
                continue;
            }

            aliveSet.Add(pickup);

            bool onScreen = OffScreenIndicatorBase.IsOnScreen(mainCamera, pickup.transform.position);

            if (onScreen)
            {
                if (activeIndicators.TryGetValue(pickup, out PowerUpOffScreenIndicator existing))
                {
                    pool.Release(existing);
                    activeIndicators.Remove(pickup);
                }

                continue;
            }

            if (!activeIndicators.TryGetValue(pickup, out PowerUpOffScreenIndicator indicator))
            {
                indicator = pool.Get();
                if (indicator != null)
                {
                    indicator.transform.SetParent(transform, false);
                    indicator.SetTarget(pickup);
                    activeIndicators[pickup] = indicator;
                }
            }

            indicator?.UpdateIndicator(mainCamera, canvasRect, edgePadding);
        }

        // Release indicators for pickups that are no longer alive
        removeList.Clear();
        foreach (KeyValuePair<PowerUpPickup, PowerUpOffScreenIndicator> kvp in activeIndicators)
        {
            if (kvp.Key == null || !aliveSet.Contains(kvp.Key))
            {
                removeList.Add(kvp.Key);
            }
        }

        for (int i = 0; i < removeList.Count; i++)
        {
            PowerUpPickup key = removeList[i];
            if (activeIndicators.TryGetValue(key, out PowerUpOffScreenIndicator ind))
            {
                pool.Release(ind);
            }

            activeIndicators.Remove(key);
        }

    }

    private void ReleaseAll()
    {
        if (pool == null)
        {
            return;
        }

        foreach (KeyValuePair<PowerUpPickup, PowerUpOffScreenIndicator> kvp in activeIndicators)
        {
            if (kvp.Value != null)
            {
                pool.Release(kvp.Value);
            }
        }

        activeIndicators.Clear();
    }
}
