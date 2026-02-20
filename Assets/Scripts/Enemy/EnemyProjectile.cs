using UnityEngine;

/// <summary>
/// Enemy projectile that moves in a direction and damages the player on contact.
/// </summary>
public class EnemyProjectile : ProjectileBase
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;

    private int damage;
    private float speed;

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
        SetLifetime(lifetime);
        ResetLifetimeTimer();

        // Set velocity
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
        }

        // Rotate to face direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    public override void OnDespawn()
    {
        base.OnDespawn();

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
        if (HasExpired())
        {
            DespawnSelf();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<ShieldBlocker>() != null)
        {
            PlayHitSfx();
            DespawnSelf();
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            // Ignore enemy collisions
            return;
        }

        if (other.CompareTag("Player"))
        {
            // Damage player on contact
            PlayerController player = other.GetComponent<PlayerController>();
            player?.TakeDamage(damage);
        }

        PlayHitSfx();
        DespawnSelf();
    }
}
