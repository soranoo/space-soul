using UnityEngine;

/// <summary>
/// Defines power-up properties using the Type Object pattern.
/// </summary>
[CreateAssetMenu(fileName = "NewPowerUpData", menuName = "Game/PowerUp Data")]
public class PowerUpData : ScriptableObject
{
    [Header("General")]
    [SerializeField] private PowerUpType powerUpType = 0;
    [SerializeField] private string displayName = "";
    [SerializeField] private Sprite icon;
    [SerializeField] private float duration = 5f;
    [Range(0f, 1f)]
    [SerializeField] private float spawnChance = 0.1f;

    [Header("Effect Values")]
    [Tooltip("Multiplier for speed/fire rate/damage power-ups.")]
    [SerializeField] private float multiplier = 1.5f;

    [Tooltip("Shield points to absorb damage.")]
    [SerializeField] private int shieldAmount = 1;

    [Header("Engine FX")]
    [Tooltip("If true, this power-up will override the player engine type while active.")]
    [SerializeField] private bool overrideEngineType;

    [SerializeField] private EngineType engineType = 0;

    [Header("Shield FX")]
    [Tooltip("If true, this power-up will override the player shield type while active.")]
    [SerializeField] private bool overrideShieldType;

    [SerializeField] private ShieldType shieldType = 0;

    [Header("Weapon FX")]
    [Tooltip("If true, this power-up will override the player weapon type while active.")]
    [SerializeField] private bool overrideWeaponType;

    [SerializeField] private WeaponType weaponType = 0;

    public PowerUpType PowerUpType => powerUpType;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public float Duration => duration;
    public float SpawnChance => spawnChance;
    public float Multiplier => multiplier;
    public int ShieldAmount => shieldAmount;
    public bool OverrideEngineType => overrideEngineType;
    public EngineType EngineType => engineType;
    public bool OverrideShieldType => overrideShieldType;
    public ShieldType ShieldType => shieldType;
    public bool OverrideWeaponType => overrideWeaponType;
    public WeaponType WeaponType => weaponType;
}
