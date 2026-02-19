using UnityEngine;

/// <summary>
/// Shared pooled projectile behavior for lifetime, despawn, and optional hit SFX.
/// </summary>
public abstract class ProjectileBase : MonoBehaviour, IPoolable
{
    [SerializeField] private string poolIdOverride;

    [Header("Projectile")]
    [SerializeField] private float lifetime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioSettings onHitSfx;

    private float spawnTime;

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
    public virtual void OnSpawn()
    {
        ResetLifetimeTimer();
    }

    /// <summary>
    /// Called when returned to pool.
    /// </summary>
    public virtual void OnDespawn()
    {
    }

    protected virtual void OnEnable()
    {
        ResetLifetimeTimer();
    }

    protected void SetLifetime(float seconds)
    {
        lifetime = Mathf.Max(0f, seconds);
    }

    protected void ResetLifetimeTimer()
    {
        spawnTime = Time.time;
    }

    protected bool HasExpired()
    {
        return Time.time >= spawnTime + lifetime;
    }

    protected void PlayHitSfx()
    {
        if (onHitSfx != null)
        {
            SfxManager.Instance?.Play(onHitSfx);
        }
    }

    protected void DespawnSelf()
    {
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