using UnityEngine;

/// <summary>
/// Manages player weapon firing.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;

    private PlayerStats stats;
    private float lastFireTime;
    private AudioSource audioSource;
    private float damageMultiplier = 1f;

    /// <summary>
    /// Initialize with player stats.
    /// </summary>
    public void Initialize(PlayerStats stats)
    {
        this.stats = stats;
    }

    /// <summary>
    /// Current damage multiplier applied to all shots.
    /// </summary>
    public float DamageMultiplier => damageMultiplier;

    /// <summary>
    /// Set damage multiplier applied to all shots.
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // If firePoints array is not set up, use transform as fallback
        if (firePoints == null || firePoints.Length == 0)
        {
            firePoints = new Transform[] { transform };
        }
    }

    /// <summary>
    /// Whether the weapon can fire (cooldown elapsed).
    /// </summary>
    public bool CanFire()
    {
        if (stats == null)
        {
            return false;
        }

        return Time.time >= lastFireTime + stats.FireRate;
    }

    /// <summary>
    /// Fire a single projectile from a random firing point.
    /// </summary>
    public void Fire()
    {
        if (!CanFire())
        {
            return;
        }

        Transform selectedFirePoint = GetRandomFirePoint();
        SpawnBullet(selectedFirePoint.position, selectedFirePoint.rotation, 1f);
        lastFireTime = Time.time;
        PlayFireSound();
    }

    /// <summary>
    /// Fire multiple projectiles in a spread pattern from each firing point.
    /// </summary>
    public void FireSpread(int count, float spreadAngle)
    {
        if (!CanFire())
        {
            return;
        }

        float halfSpread = spreadAngle * (count - 1) / 2f;

        for (int i = 0; i < count; i++)
        {
            float angle = -halfSpread + (spreadAngle * i);
            Transform selectedFirePoint = GetRandomFirePoint();
            Quaternion rotation = selectedFirePoint.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnBullet(selectedFirePoint.position, rotation, 1f);
        }

        lastFireTime = Time.time;
        PlayFireSound();
    }

    /// <summary>
    /// Fire a charged projectile with damage multiplier from a random firing point.
    /// </summary>
    public void FireCharged(float chargeMultiplier)
    {
        Transform selectedFirePoint = GetRandomFirePoint();
        SpawnBullet(selectedFirePoint.position, selectedFirePoint.rotation, chargeMultiplier);
        lastFireTime = Time.time;
        PlayFireSound();
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation, float damageMultiplier)
    {
        Bullet bullet = null;

        // Try to get from pool first
        if (PoolManager.Instance != null && bulletPrefab != null)
        {
            Bullet prefabComponent = bulletPrefab.GetComponent<Bullet>();
            if (prefabComponent != null)
            {
                bullet = PoolManager.Instance.Get(prefabComponent, position, rotation);
            }
            else
            {
                Debug.LogWarning("WeaponController: bulletPrefab does not have a Bullet component.");
            }
        }

        // Fallback to instantiation if pool not available
        if (bullet == null && bulletPrefab != null)
        {
            GameObject bulletObject = Instantiate(bulletPrefab, position, rotation);
            bullet = bulletObject.GetComponent<Bullet>();
        }

        if (bullet != null)
        {
            float finalMultiplier = this.damageMultiplier * damageMultiplier;
            bullet.SetDamageMultiplier(finalMultiplier);
        }
        else
        {
            Debug.LogWarning("Failed to spawn bullet. Ensure bullet pool is configured or bulletPrefab is assigned.");
        }
    }

    private void PlayFireSound()
    {
        if (fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }

    /// <summary>
    /// Get a random firing point from the available firing points.
    /// </summary>
    private Transform GetRandomFirePoint()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
            return transform;
        }

        int randomIndex = Random.Range(0, firePoints.Length);
        return firePoints[randomIndex] ?? transform;
    }
}
