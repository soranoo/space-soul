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
}
