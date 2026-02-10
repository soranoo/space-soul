using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active power-ups on the player.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PowerUpController : MonoBehaviour
{
    public struct PowerUpStatus
    {
        public PowerUpType Type;
        public PowerUpData Data;
        public float Remaining;
        public float Duration;
    }

    private class ActivePowerUp
    {
        public PowerUpType Type;
        public PowerUpData Data;
        public IPowerUp Effect;
        public float Duration;
        public float Remaining;
        public bool OverridesEngine;
        public EngineType PreviousEngine;
        public bool OverridesShield;
        public ShieldType PreviousShield;
        public bool OverridesWeapon;
        public WeaponType PreviousWeapon;
    }

    [Header("References")]
    [SerializeField] private PlayerEngineController engineController;
    [SerializeField] private PlayerShieldController shieldController;
    [SerializeField] private PlayerWeaponManager weaponManager;

    private PlayerController player;

    private readonly List<ActivePowerUp> activePowerUps = new List<ActivePowerUp>();
    private int shieldPoints;

    public event Action<PowerUpData, float> PowerUpApplied;
    public event Action<PowerUpType> PowerUpExpired;

    public void GetActivePowerUps(List<PowerUpStatus> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();
        for (int i = 0; i < activePowerUps.Count; i++)
        {
            ActivePowerUp active = activePowerUps[i];
            results.Add(new PowerUpStatus
            {
                Type = active.Type,
                Data = active.Data,
                Remaining = active.Remaining,
                Duration = active.Duration
            });
        }
    }

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        if (engineController == null)
        {
            engineController = GetComponentInChildren<PlayerEngineController>(true);
        }

        if (shieldController == null)
        {
            shieldController = GetComponentInChildren<PlayerShieldController>(true);
        }

        if (weaponManager == null)
        {
            weaponManager = GetComponentInChildren<PlayerWeaponManager>(true);
        }
    }

    private void Update()
    {
        if (activePowerUps.Count == 0)
        {
            return;
        }

        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            ActivePowerUp active = activePowerUps[i];
            active.Remaining -= Time.deltaTime;
            if (active.Remaining <= 0f)
            {
                active.Effect.Deactivate(player);
                if (active.OverridesEngine && engineController != null)
                {
                    engineController.SetEngineType(active.PreviousEngine);
                }
                if (active.OverridesShield && shieldController != null)
                {
                    shieldController.SetShieldType(active.PreviousShield);
                }
                if (active.OverridesWeapon && weaponManager != null)
                {
                    weaponManager.SetWeaponType(active.PreviousWeapon);
                }
                activePowerUps.RemoveAt(i);
                PowerUpExpired?.Invoke(active.Type);
            }
        }
    }

    /// <summary>
    /// Apply a new power-up effect.
    /// </summary>
    public void ApplyPowerUp(PowerUpData data)
    {
        if (data == null || player == null)
        {
            return;
        }

        IPowerUp effect = CreateEffect(data);
        if (effect == null)
        {
            return;
        }

        float duration = effect.GetDuration();

        // If same type already active, refresh duration instead of stacking.
        for (int i = 0; i < activePowerUps.Count; i++)
        {
            if (activePowerUps[i].Type == data.PowerUpType)
            {
                activePowerUps[i].Remaining = duration;
                activePowerUps[i].Duration = duration;
                activePowerUps[i].Data = data;
                if (data.OverrideEngineType && engineController != null)
                {
                    engineController.SetEngineType(data.EngineType);
                }
                if (data.OverrideShieldType && shieldController != null)
                {
                    shieldController.SetShieldType(data.ShieldType);
                }
                if (data.OverrideWeaponType && weaponManager != null)
                {
                    weaponManager.SetWeaponType(data.WeaponType);
                }
                PowerUpApplied?.Invoke(data, duration);
                return;
            }
        }

        effect.Activate(player);
        bool overridesEngine = data.OverrideEngineType && engineController != null;
        EngineType previousEngine = overridesEngine ? engineController.CurrentEngineType : EngineType.Base;
        if (overridesEngine)
        {
            engineController.SetEngineType(data.EngineType);
        }

        bool overridesShield = data.OverrideShieldType && shieldController != null;
        ShieldType previousShield = overridesShield ? shieldController.CurrentShieldType : ShieldType.None;
        if (overridesShield)
        {
            shieldController.SetShieldType(data.ShieldType);
        }

        bool overridesWeapon = data.OverrideWeaponType && weaponManager != null;
        WeaponType previousWeapon = overridesWeapon ? weaponManager.CurrentWeaponType : WeaponType.AutoCannon;
        if (overridesWeapon)
        {
            weaponManager.SetWeaponType(data.WeaponType);
        }

        activePowerUps.Add(new ActivePowerUp
        {
            Type = data.PowerUpType,
            Data = data,
            Effect = effect,
            Duration = duration,
            Remaining = duration,
            OverridesEngine = overridesEngine,
            PreviousEngine = previousEngine,
            OverridesShield = overridesShield,
            PreviousShield = previousShield,
            OverridesWeapon = overridesWeapon,
            PreviousWeapon = previousWeapon
        });

        PowerUpApplied?.Invoke(data, duration);
    }

    /// <summary>
    /// Process incoming damage using shield points if present.
    /// Returns remaining damage after shield absorption.
    /// </summary>
    public int ProcessIncomingDamage(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        if (shieldPoints <= 0)
        {
            return amount;
        }

        int absorbed = Mathf.Min(shieldPoints, amount);
        shieldPoints -= absorbed;
        return amount - absorbed;
    }

    public void AddShield(int amount)
    {
        if (amount > 0)
        {
            shieldPoints += amount;
        }
    }

    public void ClearShield()
    {
        shieldPoints = 0;
    }

    private IPowerUp CreateEffect(PowerUpData data)
    {
        switch (data.PowerUpType)
        {
            case PowerUpType.Engine:
                return new SpeedBoostPowerUp(data.Duration, data.Multiplier);
            case PowerUpType.Weapon:
                return new RapidFirePowerUp(data.Duration, data.Multiplier);
            case PowerUpType.Shield:
                return new ShieldPowerUp(data.Duration, data.ShieldAmount, this);
            default:
                return null;
        }
    }
}
