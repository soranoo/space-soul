using UnityEngine;

/// <summary>
/// Minimal pooled particle object with automatic release after a computed lifetime.
/// Useful for one-shot VFX like damage smoke.
/// </summary>
[DisallowMultipleComponent]
public class PooledParticle : MonoBehaviour, IPoolable
{
    [SerializeField] private string poolIdOverride;

    [Header("Particle")]
    [SerializeField] private ParticleSystem particleSystemRef;
    [SerializeField] private bool useAutoLifetime = true;
    [SerializeField, Min(0.05f)] private float fallbackLifetime = 2f;

    private float despawnAtTime;

    public string GetPoolId()
    {
        if (!string.IsNullOrWhiteSpace(poolIdOverride))
        {
            return poolIdOverride;
        }

        return gameObject.name;
    }

    public void SetPoolId(string poolId)
    {
        poolIdOverride = poolId;
    }

    public void OnSpawn()
    {
        EnsureParticleReference();
        RestartAndConfigureDespawn();
    }

    public void OnDespawn()
    {
        if (particleSystemRef != null)
        {
            particleSystemRef.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Awake()
    {
        EnsureParticleReference();
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (!useAutoLifetime)
        {
            return;
        }

        if (Time.time >= despawnAtTime)
        {
            PoolManager.Instance?.Release(this);
        }
    }

    /// <summary>
    /// Attach to a parent and place in local space.
    /// Restarts the particle and refreshes despawn timing.
    /// </summary>
    public void AttachTo(Transform parent, Vector3 localPosition, Quaternion localRotation)
    {
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;

        RestartAndConfigureDespawn();
    }

    /// <summary>
    /// Enable/disable automatic lifetime-based despawn.
    /// When disabled, caller is responsible for releasing this object.
    /// </summary>
    public void SetAutoLifetime(bool enabled)
    {
        useAutoLifetime = enabled;

        if (useAutoLifetime)
        {
            despawnAtTime = Time.time + ResolveLifetime();
        }
    }

    /// <summary>
    /// Caller-controlled despawn scheduling.
    /// Useful when auto lifetime is disabled.
    /// </summary>
    public void ScheduleDespawn(float seconds)
    {
        despawnAtTime = Time.time + Mathf.Max(0.05f, seconds);
        useAutoLifetime = true;
    }

    /// <summary>
    /// Immediately return this particle to the pool.
    /// </summary>
    public void ReleaseNow()
    {
        PoolManager.Instance?.Release(this);
    }

    private void EnsureParticleReference()
    {
        if (particleSystemRef == null)
        {
            particleSystemRef = GetComponent<ParticleSystem>();
        }
    }

    private void RestartAndConfigureDespawn()
    {
        if (particleSystemRef == null)
        {
            return;
        }

        particleSystemRef.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleSystemRef.Play(true);

        if (useAutoLifetime)
        {
            despawnAtTime = Time.time + ResolveLifetime();
        }
    }

    private float ResolveLifetime()
    {
        if (particleSystemRef == null)
        {
            return fallbackLifetime;
        }

        ParticleSystem.MainModule main = particleSystemRef.main;
        float startLifetime = main.startLifetime.constantMax;
        float computed = Mathf.Max(0.05f, main.duration + startLifetime);

        if (main.loop)
        {
            return Mathf.Max(0.05f, fallbackLifetime);
        }

        return computed;
    }
}
