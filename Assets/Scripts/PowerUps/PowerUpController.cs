using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages active power-ups on the player.
/// </summary>
public class PowerUpController : MonoBehaviour
{
    private class ActivePowerUp
    {
        public PowerUpType Type;
        public IPowerUp Effect;
        public float Remaining;
        public bool OverridesEngine;
        public EngineType PreviousEngine;
    }

    [Header("References")]
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerEngineController engineController;

    private readonly List<ActivePowerUp> activePowerUps = new List<ActivePowerUp>();
    private int shieldPoints;

    private void Awake()
    {
        if (player == null)
        {
            player = GetComponent<PlayerController>();
        }

        if (engineController == null)
        {
            engineController = GetComponentInChildren<PlayerEngineController>(true);
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
                activePowerUps.RemoveAt(i);
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

        // If same type already active, refresh duration instead of stacking.
        for (int i = 0; i < activePowerUps.Count; i++)
        {
            if (activePowerUps[i].Type == data.PowerUpType)
            {
                activePowerUps[i].Remaining = effect.GetDuration();
                if (data.OverrideEngineType && engineController != null)
                {
                    engineController.SetEngineType(data.EngineType);
                }
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

        activePowerUps.Add(new ActivePowerUp
        {
            Type = data.PowerUpType,
            Effect = effect,
            Remaining = effect.GetDuration(),
            OverridesEngine = overridesEngine,
            PreviousEngine = previousEngine
        });
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
