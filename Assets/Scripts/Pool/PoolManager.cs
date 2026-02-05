using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all object pools in the game.
/// Provides centralized access to pools for different object types.
/// </summary>
public class PoolManager : SingletonBase<PoolManager>
{
    [Header("Pool Settings")]
    [SerializeField] private PoolConfig[] poolConfigs;

    private Dictionary<string, object> pools;
    private Transform poolContainer;

    /// <summary>
    /// Configuration for a single pool.
    /// </summary>
    [Serializable]
    public class PoolConfig
    {
        [Tooltip("Prefab to pool.")]
        public GameObject prefab;

        [Tooltip("Initial number of objects to create.")]
        public int initialSize = 10;

        [Tooltip("Can the pool grow beyond initial size?")]
        public bool canExpand = true;
    }

    protected override void Awake()
    {
        base.Awake();
        pools = new Dictionary<string, object>();

        // Create container for pooled objects
        poolContainer = new GameObject("PoolContainer").transform;
        poolContainer.SetParent(transform);

        InitializePools();
    }

    /// <summary>
    /// Initialize all configured pools.
    /// </summary>
    private void InitializePools()
    {
        if (poolConfigs == null)
        {
            return;
        }

        for (int i = 0; i < poolConfigs.Length; i++)
        {
            PoolConfig config = poolConfigs[i];
            if (config.prefab == null)
            {
                Debug.LogWarning($"Pool config at index {i} has no prefab assigned.");
                continue;
            }

            CreatePoolFromConfig(config);
        }
    }

    /// <summary>
    /// Create a pool from configuration.
    /// </summary>
    private void CreatePoolFromConfig(PoolConfig config)
    {
        // Get the IPoolable component from the prefab
        IPoolable poolable = config.prefab.GetComponent<IPoolable>();
        if (poolable == null)
        {
            Debug.LogError($"Prefab '{config.prefab.name}' does not implement IPoolable.");
            return;
        }

        if (!TryGetPoolId(poolable, out string poolId))
        {
            Debug.LogError($"Prefab '{config.prefab.name}' has an invalid pool id.");
            return;
        }

        if (pools.ContainsKey(poolId))
        {
            Debug.LogWarning($"Pool '{poolId}' already exists. Skipping duplicate prefab '{config.prefab.name}'.");
            return;
        }

        // Create pool container for this type
        Transform typeContainer = new GameObject(poolId).transform;
        typeContainer.SetParent(poolContainer);

        // Use reflection to create the generic pool
        Type poolableType = poolable.GetType();
        Type poolType = typeof(ObjectPool<>).MakeGenericType(poolableType);

        // Get the component to pass to the pool
        Component prefabComponent = config.prefab.GetComponent(poolableType);

        object pool = Activator.CreateInstance(
            poolType,
            prefabComponent,
            config.initialSize,
            typeContainer,
            config.canExpand
        );

        pools[poolId] = pool;
    }

    /// <summary>
    /// Register a pool at runtime.
    /// </summary>
    /// <typeparam name="T">Type of poolable object.</typeparam>
    /// <param name="prefab">Prefab to pool.</param>
    /// <param name="initialSize">Initial pool size.</param>
    /// <param name="canExpand">Can pool grow beyond initial size.</param>
    public void RegisterPool<T>(T prefab, int initialSize = 10, bool canExpand = true)
        where T : MonoBehaviour, IPoolable
    {
        if (!TryGetPoolId(prefab, out string poolId))
        {
            Debug.LogError("Prefab has an invalid pool id.");
            return;
        }

        if (pools.ContainsKey(poolId))
        {
            Debug.LogWarning($"Pool '{poolId}' already exists.");
            return;
        }

        Transform typeContainer = new GameObject(poolId).transform;
        typeContainer.SetParent(poolContainer);

        ObjectPool<T> pool = new ObjectPool<T>(prefab, initialSize, typeContainer, canExpand);
        pools[poolId] = pool;
    }

    /// <summary>
    /// Get a pool by prefab instance.
    /// </summary>
    public ObjectPool<T> GetPool<T>(T prefab) where T : MonoBehaviour, IPoolable
    {
        if (!TryGetPoolId(prefab, out string poolId))
        {
            Debug.LogWarning("Prefab has an invalid pool id.");
            return null;
        }

        return GetPool<T>(poolId);
    }

    /// <summary>
    /// Get a pool by its identifier.
    /// </summary>
    /// <typeparam name="T">Type of poolable object.</typeparam>
    /// <param name="poolId">Pool identifier.</param>
    /// <returns>The object pool, or null if not found.</returns>
    public ObjectPool<T> GetPool<T>(string poolId) where T : MonoBehaviour, IPoolable
    {
        if (string.IsNullOrWhiteSpace(poolId))
        {
            Debug.LogWarning("Pool id is null or empty.");
            return null;
        }

        if (pools.TryGetValue(poolId, out object pool))
        {
            return pool as ObjectPool<T>;
        }

        Debug.LogWarning($"Pool '{poolId}' not found.");
        return null;
    }

    /// <summary>
    /// Get an object from a pool.
    /// </summary>
    /// <typeparam name="T">Type of poolable object.</typeparam>
    /// <param name="poolId">Pool identifier.</param>
    /// <returns>An active pooled object, or null if pool not found or exhausted.</returns>
    public T Get<T>(string poolId) where T : MonoBehaviour, IPoolable
    {
        ObjectPool<T> pool = GetPool<T>(poolId);
        return pool?.Get();
    }

    /// <summary>
    /// Get an object from a pool using the prefab's pool id.
    /// </summary>
    public T Get<T>(T prefab) where T : MonoBehaviour, IPoolable
    {
        ObjectPool<T> pool = GetPool(prefab);
        return pool?.Get();
    }

    /// <summary>
    /// Get an object from a pool and position it.
    /// </summary>
    /// <typeparam name="T">Type of poolable object.</typeparam>
    /// <param name="poolId">Pool identifier.</param>
    /// <param name="position">World position.</param>
    /// <param name="rotation">Rotation.</param>
    /// <returns>An active pooled object, or null if pool not found or exhausted.</returns>
    public T Get<T>(string poolId, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolable
    {
        ObjectPool<T> pool = GetPool<T>(poolId);
        return pool?.Get(position, rotation);
    }

    /// <summary>
    /// Get an object from a pool using the prefab's pool id and position it.
    /// </summary>
    public T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour, IPoolable
    {
        ObjectPool<T> pool = GetPool(prefab);
        return pool?.Get(position, rotation);
    }

    /// <summary>
    /// Return an object to its pool.
    /// </summary>
    /// <typeparam name="T">Type of poolable object.</typeparam>
    /// <param name="poolId">Pool identifier.</param>
    /// <param name="instance">Object to return.</param>
    public void Release<T>(string poolId, T instance) where T : MonoBehaviour, IPoolable
    {
        ObjectPool<T> pool = GetPool<T>(poolId);
        pool?.Release(instance);
    }

    /// <summary>
    /// Return an object to its pool using the instance's pool id.
    /// </summary>
    public void Release<T>(T instance) where T : MonoBehaviour, IPoolable
    {
        if (!TryGetPoolId(instance, out string poolId))
        {
            Debug.LogWarning("Instance has an invalid pool id.");
            return;
        }

        Release(poolId, instance);
    }

    private static bool TryGetPoolId(IPoolable poolable, out string poolId)
    {
        poolId = poolable?.GetPoolId();
        return !string.IsNullOrWhiteSpace(poolId);
    }

    /// <summary>
    /// Release all objects in a specific pool.
    /// </summary>
    /// <typeparam name="T">Type of poolable object.</typeparam>
    /// <param name="poolId">Pool identifier.</param>
    public void ReleaseAll<T>(string poolId) where T : MonoBehaviour, IPoolable
    {
        ObjectPool<T> pool = GetPool<T>(poolId);
        pool?.ReleaseAll();
    }

    /// <summary>
    /// Release all objects in all pools.
    /// </summary>
    public void ReleaseAllPools()
    {
        List<KeyValuePair<string, object>> poolEntries = new List<KeyValuePair<string, object>>(pools);
        for (int i = 0; i < poolEntries.Count; i++)
        {
            // Use reflection to call ReleaseAll on each pool
            object pool = poolEntries[i].Value;
            System.Reflection.MethodInfo releaseAllMethod = pool.GetType().GetMethod("ReleaseAll");
            releaseAllMethod?.Invoke(pool, null);
        }
    }

    /// <summary>
    /// Get pool statistics for debugging.
    /// </summary>
    /// <returns>Dictionary of pool id to (active, inactive, total) counts.</returns>
    public Dictionary<string, (int active, int inactive, int total)> GetPoolStats()
    {
        Dictionary<string, (int, int, int)> stats = new Dictionary<string, (int, int, int)>();

        List<KeyValuePair<string, object>> poolEntries = new List<KeyValuePair<string, object>>(pools);
        for (int i = 0; i < poolEntries.Count; i++)
        {
            object pool = poolEntries[i].Value;
            System.Reflection.PropertyInfo activeProperty = pool.GetType().GetProperty("ActiveCount");
            System.Reflection.PropertyInfo inactiveProperty = pool.GetType().GetProperty("InactiveCount");
            System.Reflection.PropertyInfo totalProperty = pool.GetType().GetProperty("TotalCount");

            int active = (int)(activeProperty?.GetValue(pool) ?? 0);
            int inactive = (int)(inactiveProperty?.GetValue(pool) ?? 0);
            int total = (int)(totalProperty?.GetValue(pool) ?? 0);

            stats[poolEntries[i].Key] = (active, inactive, total);
        }

        return stats;
    }

    private void OnDestroy()
    {
        // Clear all pools
        List<KeyValuePair<string, object>> poolEntries = new List<KeyValuePair<string, object>>(pools);
        for (int i = 0; i < poolEntries.Count; i++)
        {
            object pool = poolEntries[i].Value;
            System.Reflection.MethodInfo clearMethod = pool.GetType().GetMethod("Clear");
            clearMethod?.Invoke(pool, null);
        }

        pools.Clear();
    }
}
