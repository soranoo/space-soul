using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages player weapons, swapping between types and firing based on input.
/// Attach to the weapon pivot.
/// </summary>
public class PlayerWeaponManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Weapon Prefabs")]
    [SerializeField] private GameObject autoCannonPrefab;
    [SerializeField] private GameObject bigSpaceGunPrefab;
    [SerializeField] private GameObject rocketsPrefab;
    [SerializeField] private GameObject zapperPrefab;

    [Header("Settings")]
    [SerializeField] private WeaponType defaultWeaponType = WeaponType.AutoCannon;

    private readonly Dictionary<WeaponType, WeaponController> spawnedWeapons = new Dictionary<WeaponType, WeaponController>();
    private WeaponType currentWeaponType;
    private WeaponController currentWeapon;
    private bool isFiring;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponentInParent<PlayerController>();
        }

        SpawnAllWeapons();
        SetWeaponType(defaultWeaponType);
    }

    private void Start()
    {
        InitializeWeapons();
    }

    private void OnEnable()
    {
        InputHandler handler = player != null ? player.InputHandler : null;
        if (handler != null)
        {
            handler.FireStarted += OnFireStarted;
            handler.FireStopped += OnFireStopped;
        }
    }

    private void OnDisable()
    {
        InputHandler handler = player != null ? player.InputHandler : null;
        if (handler != null)
        {
            handler.FireStarted -= OnFireStarted;
            handler.FireStopped -= OnFireStopped;
        }
    }

    private void Update()
    {
        if (isFiring && currentWeapon != null)
        {
            currentWeapon.Fire();
        }
    }

    /// <summary>
    /// Switch to a new weapon type.
    /// </summary>
    public void SetWeaponType(WeaponType weaponType)
    {
        if (currentWeaponType == weaponType && currentWeapon != null)
        {
            return;
        }

        DeactivateCurrentWeapon();

        currentWeapon = GetWeapon(weaponType);
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(true);
        }

        currentWeaponType = weaponType;
    }

    public WeaponType CurrentWeaponType => currentWeaponType;

    private void InitializeWeapons()
    {
        if (player == null)
        {
            return;
        }

        PlayerStats stats = player.Stats;
        foreach (WeaponController weapon in spawnedWeapons.Values)
        {
            if (weapon != null)
            {
                weapon.Initialize(stats);
            }
        }
    }

    private void SpawnAllWeapons()
    {
        SpawnWeapon(WeaponType.AutoCannon, autoCannonPrefab);
        SpawnWeapon(WeaponType.BigSpaceGun, bigSpaceGunPrefab);
        SpawnWeapon(WeaponType.Rockets, rocketsPrefab);
        SpawnWeapon(WeaponType.Zapper, zapperPrefab);
    }

    private void SpawnWeapon(WeaponType weaponType, GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (spawnedWeapons.ContainsKey(weaponType) && spawnedWeapons[weaponType] != null)
        {
            return;
        }

        GameObject instance = Instantiate(prefab, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.SetActive(false);

        WeaponController controller = instance.GetComponent<WeaponController>();
        if (controller == null)
        {
            Debug.LogWarning("PlayerWeaponManager: Weapon prefab is missing WeaponController component.");
            Destroy(instance);
            return;
        }

        spawnedWeapons[weaponType] = controller;
    }

    private WeaponController GetWeapon(WeaponType weaponType)
    {
        if (spawnedWeapons.TryGetValue(weaponType, out WeaponController existing) && existing != null)
        {
            return existing;
        }

        return null;
    }

    private void DeactivateCurrentWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.gameObject.SetActive(false);
        }
    }

    private void OnFireStarted()
    {
        isFiring = true;
    }

    private void OnFireStopped()
    {
        isFiring = false;
    }
}
