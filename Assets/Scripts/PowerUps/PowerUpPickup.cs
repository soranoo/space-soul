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

    private void Awake()
    {
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
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
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
}
