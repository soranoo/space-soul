using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Manages player weapon firing.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Fire Rate")]
    [Tooltip("Base cooldown between shots in seconds (before player multiplier).")]
    [SerializeField] private float baseFireRate = 0.25f;

    [Header("Audio")]
    [FormerlySerializedAs("fireSound")]
    [SerializeField] private AudioSettings fireSfxSettings;

    private PlayerStats stats;
    private float lastFireTime;
    private AudioSource audioSource;
    private float damageMultiplier = 1f;
    private bool isFiring;

    /// <summary>
    /// Initialize with player stats.
    /// </summary>
    public void Initialize(PlayerStats stats)
    {
        this.stats = stats;
    }

    /// <summary>
    /// Base cooldown between shots in seconds (before player fire rate multiplier).
    /// </summary>
    public float BaseFireRate => baseFireRate;

    /// <summary>
    /// Current damage multiplier applied to all shots.
    /// </summary>
    public float DamageMultiplier => damageMultiplier;

    protected PlayerStats Stats => stats;
    protected AudioSource WeaponAudioSource => audioSource;
    protected AudioSettings FireSfxSettings => fireSfxSettings;
    protected bool IsFiring => isFiring;

    /// <summary>
    /// Set damage multiplier applied to all shots.
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
    }

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // If firePoints array is not set up, use transform as fallback
        if (firePoints == null || firePoints.Length == 0)
        {
            firePoints = new Transform[] { transform };
        }
    }

    protected virtual void OnDisable()
    {
        isFiring = false;
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

        return Time.time >= lastFireTime + GetEffectiveCooldown();
    }

    /// <summary>
    /// Effective cooldown between shots (base fire rate * player multiplier).
    /// </summary>
    protected float GetEffectiveCooldown()
    {
        float multiplier = stats != null ? stats.FireRateMultiplier : 1f;
        return baseFireRate * multiplier;
    }

    /// <summary>
    /// Fire a single projectile from a random firing point.
    /// </summary>
    public virtual void Fire()
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

    /// <summary>
    /// Set whether the weapon is currently being fired (used for continuous weapons like zapper).
    /// </summary>
    public virtual void SetFiring(bool firing)
    {
        isFiring = firing;
    }

    protected void SetLastFireTime(float value)
    {
        lastFireTime = value;
    }

    protected void SpawnBullet(Vector3 position, Quaternion rotation, float damageMultiplier)
    {
        PlayerProjectile bullet = null;

        // Try to get from pool first
        if (bulletPrefab != null)
        {
            PlayerProjectile prefabComponent = bulletPrefab.GetComponent<PlayerProjectile>();
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
            bullet = bulletObject.GetComponent<PlayerProjectile>();
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

    protected void PlayFireSound()
    {
        if (fireSfxSettings == null || fireSfxSettings.Clip == null)
        {
            return;
        }

        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.Play(fireSfxSettings, audioSource);
            return;
        }

        if (audioSource == null)
        {
            return;
        }

        if (fireSfxSettings.Source != null)
        {
            fireSfxSettings.Source.ApplyTo(audioSource);
        }

        float volume = fireSfxSettings.Source != null ? fireSfxSettings.Source.Volume : 1f;
        audioSource.PlayOneShot(fireSfxSettings.Clip, volume);
    }

    /// <summary>
    /// Get a random firing point from the available firing points.
    /// </summary>
    protected Transform GetRandomFirePoint()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
            return transform;
        }

        int randomIndex = Random.Range(0, firePoints.Length);
        return firePoints[randomIndex] ?? transform;
    }

}
