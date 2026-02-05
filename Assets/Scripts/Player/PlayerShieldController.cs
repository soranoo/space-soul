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

    private readonly Dictionary<ShieldType, GameObject> spawnedShields = new Dictionary<ShieldType, GameObject>();
    private ShieldType currentShieldType;

    private void Awake()
    {
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

        if (shieldType == ShieldType.None)
        {
            currentShieldType = shieldType;
            return;
        }

        GameObject shieldInstance = GetShield(shieldType);
        if (shieldInstance != null)
        {
            shieldInstance.SetActive(true);
        }

        currentShieldType = shieldType;
    }

    public ShieldType CurrentShieldType => currentShieldType;

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
}
