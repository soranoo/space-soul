using UnityEngine;

/// <summary>
/// Enemy projectile that moves in a direction and damages the player on contact.
/// </summary>
public class EnemyProjectile : MonoBehaviour, IPoolable
{
    [SerializeField] private string poolIdOverride;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    private int damage;
    private float speed;
    private float lifetime;
    private float spawnTime;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// Initialize projectile with damage, speed, direction, and lifetime.
    /// </summary>
    /// <param name="damage">Damage to deal on hit.</param>
    /// <param name="speed">Movement speed.</param>
    /// <param name="direction">Normalized direction to move.</param>
    /// <param name="lifetime">Time before auto-despawn.</param>
    public void Initialize(int damage, float speed, Vector2 direction, float lifetime = 5f)
    {
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.spawnTime = Time.time;

        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }

        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
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

    public void OnSpawn()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    public void OnDespawn()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
    }

    private void Update()
    {
        // Check lifetime
        if (Time.time - spawnTime >= lifetime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ShieldBlocker>() != null)
        {
            Despawn();
            return;
        }

        // Ignore enemy collisions
        if (other.CompareTag("Enemy"))
        {
            return;
        }

        // Damage player on contact
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player?.TakeDamage(damage);
        }

        Despawn();
    }

    private void Despawn()
    {
        PoolManager.Instance.Release(this);
    }
}
