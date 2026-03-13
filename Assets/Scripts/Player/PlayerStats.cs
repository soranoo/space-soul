using UnityEngine;

/// <summary>
/// Data container for player attributes.
/// </summary>
[System.Serializable]
public class PlayerStats
{
    [Header("Movement")]
    [SerializeField] private float thrustForceMultiplier = 1f;
    [SerializeField] private float rotationSpeedMultiplier = 1f;
    [SerializeField] private float maxSpeedMultiplier = 1f;

    [Header("Combat")]
    [Tooltip("Multiplier applied to all weapon cooldowns. Lower = faster. 1 = normal.")]
    [SerializeField] private float fireRateMultiplier = 1f;
    [SerializeField] private int maxHealth = 3;

    [Header("Regeneration")]
    [Tooltip("Health points restored per second. 0 = no regen.")]
    [SerializeField] private float healthRegenRate = 0f;

    /// <summary>
    /// Multiplier applied to engine thrust force.
    /// </summary>
    public float ThrustForceMultiplier => thrustForceMultiplier;

    /// <summary>
    /// Multiplier applied to engine rotation speed.
    /// </summary>
    public float RotationSpeedMultiplier => rotationSpeedMultiplier;

    /// <summary>
    /// Multiplier applied to engine max speed.
    /// </summary>
    public float MaxSpeedMultiplier => maxSpeedMultiplier;

    /// <summary>
    /// Multiplier applied to all weapon cooldowns (lower = faster firing).
    /// </summary>
    public float FireRateMultiplier => fireRateMultiplier;

    /// <summary>
    /// Maximum health points.
    /// </summary>
    public int MaxHealth => maxHealth;

    /// <summary>
    /// Health points restored per second (0 = disabled).
    /// </summary>
    public float HealthRegenRate => healthRegenRate;

    /// <summary>
    /// Apply a multiplicative modifier to thrust force multiplier.
    /// </summary>
    public void ModifyThrustForceMultiplier(float multiplier)
    {
        thrustForceMultiplier = Mathf.Max(0f, thrustForceMultiplier * multiplier);
    }

    /// <summary>
    /// Apply a multiplicative modifier to rotation speed multiplier.
    /// </summary>
    public void ModifyRotationSpeedMultiplier(float multiplier)
    {
        rotationSpeedMultiplier = Mathf.Max(0f, rotationSpeedMultiplier * multiplier);
    }

    /// <summary>
    /// Apply a multiplicative modifier to max speed multiplier.
    /// </summary>
    public void ModifyMaxSpeedMultiplier(float multiplier)
    {
        maxSpeedMultiplier = Mathf.Max(0f, maxSpeedMultiplier * multiplier);
    }

    /// <summary>
    /// Apply a modifier to the fire rate multiplier (multiplicative).
    /// </summary>
    public void ModifyFireRate(float multiplier)
    {
        fireRateMultiplier *= multiplier;
    }

    /// <summary>
    /// Increase max health by amount.
    /// </summary>
    public void IncreaseMaxHealth(int amount)
    {
        maxHealth += amount;
    }

    /// <summary>
    /// Add to health regen rate (HP per second).
    /// </summary>
    public void IncreaseHealthRegenRate(float amount)
    {
        healthRegenRate += amount;
    }

    /// <summary>
    /// Increase rotation speed multiplier by a flat amount.
    /// </summary>
    public void IncreaseRotationSpeedMultiplier(float amount)
    {
        rotationSpeedMultiplier = Mathf.Max(0f, rotationSpeedMultiplier + amount);
    }

    /// <summary>
    /// Decrease the fire rate multiplier by a flat amount (clamped to min 0.2).
    /// </summary>
    public void DecreaseFireRate(float amount)
    {
        fireRateMultiplier = Mathf.Max(0.2f, fireRateMultiplier - amount);
    }
}
