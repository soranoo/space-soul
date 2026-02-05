using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic object pool for any poolable MonoBehaviour type.
/// </summary>
/// <typeparam name="T">Type of object to pool. Must be a MonoBehaviour that implements IPoolable.</typeparam>
public class ObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly T prefab;
    private readonly Transform parent;
    private readonly Queue<T> inactiveObjects;
    private readonly HashSet<T> activeObjects;
    private readonly int initialSize;
    private readonly bool canExpand;

    /// <summary>
    /// Number of currently active objects from this pool.
    /// </summary>
    public int ActiveCount => activeObjects.Count;

    /// <summary>
    /// Number of currently inactive objects available in this pool.
    /// </summary>
    public int InactiveCount => inactiveObjects.Count;

    /// <summary>
    /// Total number of objects created by this pool.
    /// </summary>
    public int TotalCount => ActiveCount + InactiveCount;

    /// <summary>
    /// Creates a new object pool.
    /// </summary>
    /// <param name="prefab">The prefab to instantiate.</param>
    /// <param name="initialSize">Number of objects to pre-instantiate.</param>
    /// <param name="parent">Optional parent transform to organize pooled objects.</param>
    /// <param name="canExpand">If true, pool will grow when exhausted. If false, Get() returns null when empty.</param>
    public ObjectPool(T prefab, int initialSize, Transform parent = null, bool canExpand = true)
    {
        this.prefab = prefab;
        this.initialSize = initialSize;
        this.parent = parent;
        this.canExpand = canExpand;

        inactiveObjects = new Queue<T>(initialSize);
        activeObjects = new HashSet<T>();

        Prewarm();
    }

    /// <summary>
    /// Pre-instantiate objects to fill the pool.
    /// </summary>
    private void Prewarm()
    {
        for (int i = 0; i < initialSize; i++)
        {
            T instance = CreateInstance();
            instance.gameObject.SetActive(false);
            inactiveObjects.Enqueue(instance);
        }
    }

    /// <summary>
    /// Create a new instance of the pooled object.
    /// </summary>
    private T CreateInstance()
    {
        T instance = UnityEngine.Object.Instantiate(prefab, parent);
        instance.name = $"{prefab.name}_{TotalCount}";
        instance.SetPoolId(prefab.GetPoolId());
        return instance;
    }

    /// <summary>
    /// Retrieve an object from the pool.
    /// </summary>
    /// <returns>An active pooled object, or null if pool is exhausted and cannot expand.</returns>
    public T Get()
    {
        T instance;

        if (inactiveObjects.Count > 0)
        {
            instance = inactiveObjects.Dequeue();
        }
        else if (canExpand)
        {
            instance = CreateInstance();
        }
        else
        {
            return null;
        }

        activeObjects.Add(instance);
        instance.gameObject.SetActive(true);
        instance.OnSpawn();

        return instance;
    }

    /// <summary>
    /// Retrieve an object from the pool and position it.
    /// </summary>
    /// <param name="position">World position for the object.</param>
    /// <param name="rotation">Rotation for the object.</param>
    /// <returns>An active pooled object, or null if pool is exhausted and cannot expand.</returns>
    public T Get(Vector3 position, Quaternion rotation)
    {
        T instance = Get();

        if (instance != null)
        {
            instance.transform.SetPositionAndRotation(position, rotation);
        }

        return instance;
    }

    /// <summary>
    /// Return an object to the pool.
    /// </summary>
    /// <param name="instance">The object to return.</param>
    public void Release(T instance)
    {
        if (instance == null)
        {
            return;
        }

        if (!activeObjects.Contains(instance))
        {
            Debug.LogWarning($"Trying to release object {instance.name} that is not managed by this pool.");
            return;
        }

        activeObjects.Remove(instance);
        instance.OnDespawn();
        instance.gameObject.SetActive(false);
        inactiveObjects.Enqueue(instance);
    }

    /// <summary>
    /// Return all active objects to the pool.
    /// </summary>
    public void ReleaseAll()
    {
        // Create a copy to avoid modifying collection during iteration
        T[] activeArray = new T[activeObjects.Count];
        activeObjects.CopyTo(activeArray);
        for (int i = 0; i < activeArray.Length; i++)
        {
            Release(activeArray[i]);
        }
    }

    /// <summary>
    /// Destroy all objects in the pool.
    /// </summary>
    public void Clear()
    {
        T[] activeArray = new T[activeObjects.Count];
        activeObjects.CopyTo(activeArray);
        for (int i = 0; i < activeArray.Length; i++)
        {
            T instance = activeArray[i];
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
            }
        }

        while (inactiveObjects.Count > 0)
        {
            T instance = inactiveObjects.Dequeue();
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance.gameObject);
            }
        }

        activeObjects.Clear();
    }
}
