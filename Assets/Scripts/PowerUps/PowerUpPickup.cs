using DG.Tweening;
using UnityEngine;

/// <summary>
/// Physical pickup in the world that applies a power-up.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PowerUpPickup : MonoBehaviour, IPoolable
{
    [Header("Power-Up")]
    [SerializeField] private PowerUpData powerUpData;

    [SerializeField] private string poolIdOverride;

    [Header("Despawn")]
    [SerializeField] private float despawnTime = 8f;
    [SerializeField] private float spawnScaleDuration = 0.2f;
    [SerializeField] private float despawnScaleDuration = 0.2f;
    [SerializeField] private float blinkStartTime = 2f;
    [SerializeField] private float blinkInterval = 0.15f;

    private Tween despawnTween;
    private Sequence blinkSequence;
    private Tween spawnTween;
    private Transform cachedTransform;
    private SpriteRenderer spriteRenderer;
    private bool spawnedFromPool;

    private void Awake()
    {
        cachedTransform = transform;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// Set the power-up data for this pickup.
    /// </summary>
    public void Initialize(PowerUpData data)
    {
        powerUpData = data;
    }

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
        spawnedFromPool = true;
        gameObject.SetActive(true);
        StartSpawnVisuals();
    }

    public void OnDespawn()
    {
        spawnTween?.Kill();
        blinkSequence?.Kill();
        despawnTween?.Kill();

        spawnedFromPool = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PowerUpController controller = other.GetComponent<PowerUpController>();
        controller?.ApplyPowerUp(powerUpData);

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (!spawnedFromPool)
        {
            StartSpawnVisuals();
        }
    }

    private void ScheduleDespawn()
    {
        despawnTween?.Kill();
        blinkSequence?.Kill();

        float blinkDelay = Mathf.Max(0f, despawnTime - blinkStartTime);
        if (spriteRenderer != null && blinkStartTime > 0f && despawnTime > 0f)
        {
            blinkSequence = DOTween.Sequence();
            blinkSequence.AppendInterval(blinkDelay);
            blinkSequence.AppendCallback(StartBlinking);
        }

        if (despawnTime > 0f)
        {
            despawnTween = DOVirtual.DelayedCall(despawnTime, DespawnWithTween);
        }
    }

    private void StartBlinking()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.DOKill();
        spriteRenderer.DOFade(0.2f, blinkInterval)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void DespawnWithTween()
    {
        blinkSequence?.Kill();
        spriteRenderer?.DOKill();

        cachedTransform.DOScale(Vector3.zero, despawnScaleDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                if (PoolManager.Instance != null)
                {
                    PoolManager.Instance.Release(this);
                }
                else
                {
                    Destroy(gameObject);
                }
            });
    }

    private void StartSpawnVisuals()
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
            spriteRenderer.enabled = true;
        }

        cachedTransform.localScale = Vector3.zero;
        spawnTween?.Kill();
        spawnTween = cachedTransform.DOScale(Vector3.one, spawnScaleDuration).SetEase(Ease.OutBack);

        ScheduleDespawn();
    }
}
