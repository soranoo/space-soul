using UnityEngine;

/// <summary>
/// Shared pooled projectile behavior for lifetime, despawn, and optional hit SFX.
/// </summary>
public abstract class ProjectileBase : MonoBehaviour, IPoolable
{
    [SerializeField] private string poolIdOverride;

    [Header("Projectile")]
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float despawnOutSideViewportMultiplier = 1f;

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

    protected virtual void Update()
    {
        if (IsOutsideViewport(despawnOutSideViewportMultiplier))
        {
            DespawnSelf();
        }
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

    /// <summary>
    /// Returns true when this projectile has moved outside a scaled camera viewport.
    /// viewportScale = 1 means exact viewport bounds.
    /// viewportScale = 1.5 means 50% larger bounds centered on the viewport.
    /// </summary>
    protected bool IsOutsideViewport(float viewportScale = 1f, Camera camera = null)
    {
        Camera targetCamera = camera != null ? camera : Camera.main;
        if (targetCamera == null)
        {
            return false;
        }

        float clampedScale = Mathf.Max(1f, viewportScale);
        float margin = (clampedScale - 1f) * 0.5f;
        float min = -margin;
        float max = 1f + margin;

        Vector3 viewportPos = targetCamera.WorldToViewportPoint(transform.position);
        if (viewportPos.z < 0f)
        {
            return true;
        }

        return viewportPos.x < min || viewportPos.x > max || viewportPos.y < min || viewportPos.y > max;
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
        PoolManager.Instance.Release(this);
    }
}