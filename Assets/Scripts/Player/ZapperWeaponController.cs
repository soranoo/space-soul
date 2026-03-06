using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Zapper-specific weapon implementation.
/// Keeps continuous laser visuals/damage separate from base projectile weapon logic.
/// </summary>
public class ZapperWeaponController : WeaponController
{
    [Header("Zapper Settings")]
    [SerializeField] private GameObject zapperSegmentPrefab;
    [SerializeField] private LayerMask zapperHitMask;
    [SerializeField] private float baseZapperLength = 6f;
    [SerializeField] private float lengthPerFireRate = 4f;
    [SerializeField] private float zapperSegmentLength = 0.5f;
    [SerializeField] private float zapperDamageInterval = 0.1f;
    [SerializeField] private int zapperDamagePerTick = 1;

    private bool isZapperLoopSfxPlaying;
    private float lastZapperDamageTime;
    private readonly List<Transform> zapperSegments = new List<Transform>();
    private readonly List<Vector3> zapperSegmentBaseScales = new List<Vector3>();

    protected override void Awake()
    {
        base.Awake();
        DisableZapperSegments();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        DisableZapperSegments();
        StopZapperLoopSfx();
    }

    public override void Fire()
    {
        FireZapper();
    }

    public override void SetFiring(bool firing)
    {
        bool wasFiring = IsFiring;
        base.SetFiring(firing);

        if (firing && !wasFiring)
        {
            StartZapperLoopSfx();
            return;
        }

        if (!firing && wasFiring)
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

        AudioSource audioSource = WeaponAudioSource;
        AudioSettings fireSettings = FireSfxSettings;
        if (audioSource == null || fireSettings == null || fireSettings.Clip == null)
        {
            return;
        }

        if (fireSettings.Source != null)
        {
            fireSettings.Source.ApplyTo(audioSource);
        }

        audioSource.clip = fireSettings.Clip;
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

        AudioSource audioSource = WeaponAudioSource;
        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
            audioSource.clip = null;
        }

        isZapperLoopSfxPlaying = false;
    }

    private void FireZapper()
    {
        if (!IsFiring)
        {
            return;
        }

        Transform selectedFirePoint = GetRandomFirePoint();
        Vector3 start = selectedFirePoint.position;
        Vector3 direction = selectedFirePoint.up;

        float fireRate = Stats != null ? Mathf.Max(0.01f, Stats.FireRate) : 0.2f;
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
                int finalDamage = Mathf.RoundToInt(zapperDamagePerTick * DamageMultiplier);
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
