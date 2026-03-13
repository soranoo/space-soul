using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager for the upgrade system.
/// Loads all UpgradeData assets, filters valid choices, and applies selected upgrades.
/// </summary>
public class UpgradeManager : SingletonBase<UpgradeManager>
{
    [Header("Settings")]
    [Tooltip("Number of upgrade cards to offer per wave.")]
    [SerializeField] private int cardsPerWave = 3;

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerEngineController engineController;
    [SerializeField] private PlayerWeaponManager weaponManager;

    private UpgradeData[] allUpgrades;
    private readonly Dictionary<UpgradeData, int> upgradeLevels = new Dictionary<UpgradeData, int>();

    /// <summary>
    /// Number of cards offered each wave.
    /// </summary>
    public int CardsPerWave => cardsPerWave;

    protected override void Awake()
    {
        base.Awake();
        allUpgrades = Resources.LoadAll<UpgradeData>("Upgrades");
    }

    /// <summary>
    /// Collect a filtered, shuffled set of valid upgrades for the current player state.
    /// </summary>
    public List<UpgradeData> GetRandomUpgrades()
    {
        List<UpgradeData> valid = new List<UpgradeData>();

        for (int i = 0; i < allUpgrades.Length; i++)
        {
            if (IsUpgradeValid(allUpgrades[i]))
            {
                valid.Add(allUpgrades[i]);
            }
        }

        ShuffleList(valid);

        int count = Mathf.Min(cardsPerWave, valid.Count);
        return valid.GetRange(0, count);
    }

    /// <summary>
    /// Apply the selected upgrade to the player permanently.
    /// </summary>
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (upgrade == null || player == null)
        {
            return;
        }

        if (!IsUpgradeValid(upgrade))
        {
            return;
        }

        PlayerStats stats = player.Stats;

        switch (upgrade.UpgradeType)
        {
            case UpgradeType.EngineUpgrade:
                if (engineController != null)
                {
                    engineController.SetEngineType(upgrade.TargetEngineType);
                }
                break;

            case UpgradeType.WeaponUpgrade:
                if (weaponManager != null)
                {
                    weaponManager.SetWeaponType(upgrade.TargetWeaponType);
                }
                break;

            case UpgradeType.RotationSpeed:
                stats.IncreaseRotationSpeedMultiplier(upgrade.StepValue);
                IncrementLevel(upgrade);
                break;

            case UpgradeType.HealthRegen:
                stats.IncreaseHealthRegenRate(upgrade.StepValue);
                IncrementLevel(upgrade);
                break;

            case UpgradeType.FullHeal:
                player.Heal(stats.MaxHealth);
                break;

            case UpgradeType.FireRate:
                stats.DecreaseFireRate(upgrade.StepValue);
                IncrementLevel(upgrade);
                break;
            case UpgradeType.MaxHealth:
                stats.IncreaseMaxHealth(Mathf.RoundToInt(upgrade.StepValue));
                IncrementLevel(upgrade);
                break;
        }
    }

    private bool IsUpgradeValid(UpgradeData upgrade)
    {
        switch (upgrade.UpgradeType)
        {
            case UpgradeType.EngineUpgrade:
                // Skip if player already has this engine type
                if (engineController != null &&
                    engineController.CurrentEngineType == upgrade.TargetEngineType)
                {
                    return false;
                }
                break;

            case UpgradeType.WeaponUpgrade:
                // Skip if player already has this weapon type
                if (weaponManager != null &&
                    weaponManager.CurrentWeaponType == upgrade.TargetWeaponType)
                {
                    return false;
                }
                break;

            case UpgradeType.FullHeal:
                // Skip if already at full health
                if (player != null &&
                    player.CurrentHealth >= player.Stats.MaxHealth)
                {
                    return false;
                }
                break;
            case UpgradeType.RotationSpeed:
            case UpgradeType.HealthRegen:
            case UpgradeType.FireRate:
            case UpgradeType.MaxHealth:
                if (upgrade.MaxLevel > 0 && GetLevel(upgrade) >= upgrade.MaxLevel)
                {
                    return false;
                }
                break;
        }

        return true;
    }

    private int GetLevel(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return 0;
        }

        if (upgradeLevels.TryGetValue(upgrade, out int level))
        {
            return level;
        }

        return 0;
    }

    private void IncrementLevel(UpgradeData upgrade)
    {
        if (upgrade == null)
        {
            return;
        }

        int level = GetLevel(upgrade);
        upgradeLevels[upgrade] = level + 1;
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
