using UnityEngine;

/// <summary>
/// Defines power-up properties using the Type Object pattern.
/// </summary>
[CreateAssetMenu(fileName = "NewPowerUpData", menuName = "Game/PowerUp Data")]
public class PowerUpData : ScriptableObject
{
    [Header("General")]
    [SerializeField] private PowerUpType powerUpType = 0;
    [SerializeField] private float duration = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.1f;

    [Header("Effect Values")]
    [Tooltip("Multiplier for speed/fire rate/damage power-ups.")]
    [SerializeField] private float multiplier = 1.5f;

    [Tooltip("Shield points to absorb damage.")]
    [SerializeField] private int shieldAmount = 1;

    public PowerUpType PowerUpType => powerUpType;
    public float Duration => duration;
    public float SpawnChance => spawnChance;
    public float Multiplier => multiplier;
    public int ShieldAmount => shieldAmount;
}
