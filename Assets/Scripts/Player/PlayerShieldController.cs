using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player shield visuals. Attach to the shield pivot.
/// </summary>
public class PlayerShieldController : MonoBehaviour
{
    [Header("Shield Prefabs")]
    [SerializeField] private GameObject frontShieldPrefab;
    [SerializeField] private GameObject frontAndSideShieldPrefab;
    [SerializeField] private GameObject invincibilityShieldPrefab;
    [SerializeField] private GameObject roundShieldPrefab;

    [Header("Settings")]
    [SerializeField] private ShieldType defaultShieldType = ShieldType.None;

    [Header("Player Collision")]
    [SerializeField] private Collider2D playerCollider;

    private readonly Dictionary<ShieldType, GameObject> spawnedShields = new Dictionary<ShieldType, GameObject>();
    private ShieldType currentShieldType;

    private void Awake()
    {
        if (playerCollider == null)
        {
            playerCollider = GetComponentInParent<Collider2D>();
        }

        SpawnAllShields();
        SetShieldType(defaultShieldType);
    }

    /// <summary>
    /// Switch to a new shield visual type.
    /// </summary>
    public void SetShieldType(ShieldType shieldType)
    {
        if (currentShieldType == shieldType)
        {
            return;
        }

        DeactivateCurrentShield();

        if (playerCollider != null)
        {
            playerCollider.enabled = shieldType != ShieldType.Invincibility;
        }

        if (shieldType == ShieldType.None)
        {
            currentShieldType = shieldType;
            return;
        }

        GameObject shieldInstance = GetShield(shieldType);
        if (shieldInstance != null)
        {
            shieldInstance.SetActive(true);
            EnsureShieldBlocker(shieldInstance);
        }

        currentShieldType = shieldType;
    }

    public ShieldType CurrentShieldType => currentShieldType;

    /// <summary>
    /// Whether a shield visual is currently active.
    /// </summary>
    public bool IsShieldActive => currentShieldType != ShieldType.None;

    /// <summary>
    /// Whether invincibility shield is active.
    /// </summary>
    public bool IsInvincible => currentShieldType == ShieldType.Invincibility;

    private void SpawnAllShields()
    {
        // None type intentionally has no prefab.
        SpawnShield(ShieldType.Front, frontShieldPrefab);
        SpawnShield(ShieldType.FrontAndSide, frontAndSideShieldPrefab);
        SpawnShield(ShieldType.Invincibility, invincibilityShieldPrefab);
        SpawnShield(ShieldType.Round, roundShieldPrefab);
    }

    private void SpawnShield(ShieldType shieldType, GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (spawnedShields.ContainsKey(shieldType) && spawnedShields[shieldType] != null)
        {
            return;
        }

        GameObject instance = Instantiate(prefab, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.SetActive(false);

        spawnedShields[shieldType] = instance;
    }

    private GameObject GetShield(ShieldType shieldType)
    {
        if (spawnedShields.TryGetValue(shieldType, out GameObject existing) && existing != null)
        {
            return existing;
        }

        return null;
    }

    private void DeactivateCurrentShield()
    {
        if (spawnedShields.TryGetValue(currentShieldType, out GameObject existing) && existing != null)
        {
            existing.SetActive(false);
        }
    }

    private void EnsureShieldBlocker(GameObject shieldInstance)
    {
        if (shieldInstance == null)
        {
            return;
        }

        if (shieldInstance.GetComponentInChildren<ShieldBlocker>() != null)
        {
            return;
        }

        Collider2D col = shieldInstance.GetComponentInChildren<Collider2D>();
        if (col != null)
        {
            col.gameObject.AddComponent<ShieldBlocker>();
        }
    }
}
