using UnityEngine;

/// <summary>
/// Data container for player attributes.
/// </summary>
[System.Serializable]
public class PlayerStats
{
    [Header("Movement")]
    [SerializeField] private float thrustForce = 10f;
    [SerializeField] private float rotationSpeed = 200f;
    [SerializeField] private float maxSpeed = 8f;

    [Header("Combat")]
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private int maxHealth = 3;

    [Header("Regeneration")]
    [Tooltip("Health points restored per second. 0 = no regen.")]
    [SerializeField] private float healthRegenRate = 0f;

    /// <summary>
    /// Force applied when thrusting forward.
    /// </summary>
    public float ThrustForce => thrustForce;

    /// <summary>
    /// Rotation torque applied when turning.
    /// </summary>
    public float RotationSpeed => rotationSpeed;

    /// <summary>
    /// Maximum movement speed.
    /// </summary>
    public float MaxSpeed => maxSpeed;

    /// <summary>
    /// Time between shots in seconds.
    /// </summary>
    public float FireRate => fireRate;

    /// <summary>
    /// Maximum health points.
    /// </summary>
    public int MaxHealth => maxHealth;

    /// <summary>
    /// Health points restored per second (0 = disabled).
    /// </summary>
    public float HealthRegenRate => healthRegenRate;

    /// <summary>
    /// Apply a modifier to thrust force.
    /// </summary>
    public void ModifyThrustForce(float multiplier)
    {
        thrustForce *= multiplier;
    }

    /// <summary>
    /// Apply a modifier to rotation speed.
    /// </summary>
    public void ModifyRotationSpeed(float multiplier)
    {
        rotationSpeed *= multiplier;
    }

    /// <summary>
    /// Apply a modifier to max speed.
    /// </summary>
    public void ModifyMaxSpeed(float multiplier)
    {
        maxSpeed *= multiplier;
    }

    /// <summary>
    /// Apply a modifier to fire rate.
    /// </summary>
    public void ModifyFireRate(float multiplier)
    {
        fireRate *= multiplier;
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
    /// Increase rotation speed by a flat amount.
    /// </summary>
    public void IncreaseRotationSpeed(float amount)
    {
        rotationSpeed += amount;
    }

    /// <summary>
    /// Decrease fire rate cooldown by a flat amount (clamped to min 0.05).
    /// </summary>
    public void DecreaseFireRate(float amount)
    {
        fireRate = Mathf.Max(0.05f, fireRate - amount);
    }
}
