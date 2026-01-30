using UnityEngine;

/// <summary>
/// Manages player weapon firing.
/// </summary>
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Audio")]
    [SerializeField] private AudioClip fireSound;

    private PlayerStats stats;
    private float lastFireTime;
    private AudioSource audioSource;

    /// <summary>
    /// Initialize with player stats.
    /// </summary>
    public void Initialize(PlayerStats stats)
    {
        this.stats = stats;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (firePoint == null)
        {
            firePoint = transform;
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
    /// Fire a single projectile.
    /// </summary>
    public void Fire()
    {
        if (!CanFire())
        {
            return;
        }

        SpawnBullet(firePoint.position, firePoint.rotation, 1f);
        lastFireTime = Time.time;
        PlayFireSound();
    }

    /// <summary>
    /// Fire multiple projectiles in a spread pattern.
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
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, angle);
            SpawnBullet(firePoint.position, rotation, 1f);
        }

        lastFireTime = Time.time;
        PlayFireSound();
    }

    /// <summary>
    /// Fire a charged projectile with damage multiplier.
    /// </summary>
    public void FireCharged(float damageMultiplier)
    {
        SpawnBullet(firePoint.position, firePoint.rotation, damageMultiplier);
        lastFireTime = Time.time;
        PlayFireSound();
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation, float damageMultiplier)
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab not assigned to WeaponController.");
            return;
        }

        // TODO: Replace with object pool once Stage 3 is implemented.
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        Bullet bulletComponent = bullet.GetComponent<Bullet>();

        if (bulletComponent != null)
        {
            bulletComponent.SetDamageMultiplier(damageMultiplier);
        }
    }

    private void PlayFireSound()
    {
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
    }
}
