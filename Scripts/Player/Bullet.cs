using UnityEngine;

/// <summary>
/// Basic bullet behavior.
/// Implements IPoolable for object pooling support.
/// </summary>
public class Bullet : MonoBehaviour, IPoolable
{
    public const string POOL_ID = "Bullet";

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
            PoolManager.Instance.Release(POOL_ID, this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
