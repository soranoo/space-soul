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

    [Header("Zapper Settings")]
    [SerializeField] private bool useZapperLaser = false;
    [SerializeField] private GameObject zapperSegmentPrefab;
    [SerializeField] private LayerMask zapperHitMask;
    [SerializeField] private float baseZapperLength = 6f;
    [SerializeField] private float lengthPerFireRate = 4f;
    [SerializeField] private float zapperSegmentLength = 0.5f;
    [SerializeField] private float zapperDamageInterval = 0.1f;
    [SerializeField] private int zapperDamagePerTick = 1;

    [Header("Audio")]
    [FormerlySerializedAs("fireSound")]
    [SerializeField] private AudioSettings fireSfxSettings;

    private PlayerStats stats;
    private float lastFireTime;
    private AudioSource audioSource;
    private float damageMultiplier = 1f;
    private bool isFiring;
    private bool isZapperLoopSfxPlaying;
    private float lastZapperDamageTime;
    private readonly List<Transform> zapperSegments = new List<Transform>();
    private readonly List<Vector3> zapperSegmentBaseScales = new List<Vector3>();

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

        if (useZapperLaser)
        {
            DisableZapperSegments();
        }
    }

    private void OnDisable()
    {
        if (useZapperLaser)
        {
            DisableZapperSegments();
            StopZapperLoopSfx();
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
        if (useZapperLaser)
        {
            FireZapper();
            return;
        }

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
        if (useZapperLaser)
        {
            FireZapper();
            return;
        }

        Transform selectedFirePoint = GetRandomFirePoint();
        SpawnBullet(selectedFirePoint.position, selectedFirePoint.rotation, chargeMultiplier);
        lastFireTime = Time.time;
        PlayFireSound();
    }

    /// <summary>
    /// Set whether the weapon is currently being fired (used for continuous weapons like zapper).
    /// </summary>
    public void SetFiring(bool firing)
    {
        bool wasFiring = isFiring;
        isFiring = firing;

        if (!useZapperLaser)
        {
            return;
        }

        if (isFiring && !wasFiring)
        {
            StartZapperLoopSfx();
            return;
        }

        if (!isFiring && wasFiring)
        {
            DisableZapperSegments();
            StopZapperLoopSfx();
        }
    }

    private void StartZapperLoopSfx()
    {
        if (isZapperLoopSfxPlaying)
        {
            return;
        }

        if (audioSource == null || fireSfxSettings == null || fireSfxSettings.Clip == null)
        {
            return;
        }

        if (fireSfxSettings.Source != null)
        {
            fireSfxSettings.Source.ApplyTo(audioSource);
        }

        audioSource.clip = fireSfxSettings.Clip;
        audioSource.loop = true;
        audioSource.Play();
        isZapperLoopSfxPlaying = true;
    }

    private void StopZapperLoopSfx()
    {
        if (!isZapperLoopSfxPlaying)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
            audioSource.clip = null;
        }

        isZapperLoopSfxPlaying = false;
    }

    private void SpawnBullet(Vector3 position, Quaternion rotation, float damageMultiplier)
    {
        Bullet bullet = null;

        // Try to get from pool first
        if (bulletPrefab != null)
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
    private Transform GetRandomFirePoint()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
            return transform;
        }

        int randomIndex = Random.Range(0, firePoints.Length);
        return firePoints[randomIndex] ?? transform;
    }

    private void FireZapper()
    {
        if (!isFiring)
        {
            return;
        }

        Transform selectedFirePoint = GetRandomFirePoint();
        Vector3 start = selectedFirePoint.position;
        Vector3 direction = selectedFirePoint.up;

        float fireRate = stats != null ? Mathf.Max(0.01f, stats.FireRate) : 0.2f;
        float maxLength = baseZapperLength + (lengthPerFireRate / fireRate);

        RaycastHit2D hit = Physics2D.Raycast(start, direction, maxLength, zapperHitMask);
        Vector3 end = hit.collider != null ? (Vector3)hit.point : start + direction * maxLength;

        float totalLength = Vector3.Distance(start, end);
        int segmentCount = zapperSegmentLength > 0f
            ? Mathf.CeilToInt(totalLength / zapperSegmentLength)
            : 0;

        EnsureZapperSegments(segmentCount);

        for (int i = 0; i < zapperSegments.Count; i++)
        {
            bool active = i < segmentCount;
            Transform segment = zapperSegments[i];
            if (segment == null)
            {
                continue;
            }

            segment.gameObject.SetActive(active);
            if (!active)
            {
                continue;
            }

            float segmentStart = zapperSegmentLength * i;
            float segmentEnd = Mathf.Min(totalLength, zapperSegmentLength * (i + 1));
            float segmentMid = (segmentStart + segmentEnd) * 0.5f;
            float segmentLength = Mathf.Max(0.01f, segmentEnd - segmentStart);

            segment.position = start + direction * segmentMid;
            segment.rotation = Quaternion.LookRotation(Vector3.forward, direction);

            Vector3 baseScale = zapperSegmentBaseScales[i];
            float lengthScale = zapperSegmentLength > 0f ? segmentLength / zapperSegmentLength : 1f;
            segment.localScale = new Vector3(baseScale.x, baseScale.y * lengthScale, baseScale.z);
        }

        if (hit.collider != null && Time.time >= lastZapperDamageTime + zapperDamageInterval)
        {
            Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                int finalDamage = Mathf.RoundToInt(zapperDamagePerTick * damageMultiplier);
                enemy.TakeDamage(finalDamage);
                lastZapperDamageTime = Time.time;
            }
        }
    }

    private void EnsureZapperSegments(int requiredCount)
    {
        if (zapperSegmentPrefab == null)
        {
            return;
        }

        while (zapperSegments.Count < requiredCount)
        {
            GameObject instance = Instantiate(zapperSegmentPrefab, transform);
            Transform segment = instance.transform;
            segment.gameObject.SetActive(false);
            zapperSegments.Add(segment);
            zapperSegmentBaseScales.Add(segment.localScale);
        }
    }

    private void DisableZapperSegments()
    {
        for (int i = 0; i < zapperSegments.Count; i++)
        {
            Transform segment = zapperSegments[i];
            if (segment != null)
            {
                segment.gameObject.SetActive(false);
            }
        }
    }
}
