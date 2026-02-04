using UnityEngine;

/// <summary>
/// Basic bullet behavior.
/// Implements IPoolable for object pooling support.
/// </summary>
public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private string poolIdOverride;

    [Header("Bullet Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int baseDamage = 1;

    private float damageMultiplier = 1f;
    private float spawnTime;

    /// <summary>
    /// Set the damage multiplier for this bullet.
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    /// <summary>
    /// Get the effective damage.
    /// </summary>
    public int GetDamage()
    {
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    /// <summary>
    /// Get the pool identifier for this prefab type.
    /// </summary>
    public string GetPoolId()
    {
        if (!string.IsNullOrWhiteSpace(poolIdOverride))
        {
            return poolIdOverride;
        }

        return gameObject.name;
    }

    /// <summary>
    /// Assign the pool identifier for this instance.
    /// </summary>
    public void SetPoolId(string poolId)
    {
        poolIdOverride = poolId;
    }

    /// <summary>
    /// Called when retrieved from pool.
    /// </summary>
    public void OnSpawn()
    {
        damageMultiplier = 1f;
        spawnTime = Time.time;
    }

    /// <summary>
    /// Called when returned to pool.
    /// </summary>
    public void OnDespawn()
    {
        // Reset state if needed
    }

    private void OnEnable()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        if (Time.time >= spawnTime + lifetime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: Add enemy collision once Stage 5 is implemented.
        if (other.CompareTag("Enemy"))
        {
            Despawn();
        }
    }

    private void Despawn()
    {
        // Use object pool if PoolManager exists, otherwise fallback to Destroy
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
