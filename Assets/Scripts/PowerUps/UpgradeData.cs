using UnityEngine;

/// <summary>
/// ScriptableObject defining a single upgrade card offered between waves.
/// </summary>
[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Display")]
    [SerializeField] private string title;
    [SerializeField] [TextArea(2, 4)] private string description;
    [SerializeField] private Sprite icon;

    [Header("Upgrade Settings")]
    [SerializeField] private UpgradeType upgradeType;

    [Tooltip("Step value used by RotationSpeed, HealthRegen, and FireRate upgrades.")]
    [SerializeField] private float stepValue;

    [Tooltip("Target engine type for EngineUpgrade.")]
    [SerializeField] private EngineType targetEngineType;

    [Tooltip("Target weapon type for WeaponUpgrade.")]
    [SerializeField] private WeaponType targetWeaponType;

    /// <summary>Card title shown at the top.</summary>
    public string Title => title;

    /// <summary>Card description shown in the body.</summary>
    public string Description => description;

    /// <summary>Card icon shown in the body.</summary>
    public Sprite Icon => icon;

    /// <summary>Which upgrade category this card belongs to.</summary>
    public UpgradeType UpgradeType => upgradeType;

    /// <summary>Numeric step value (meaning depends on UpgradeType).</summary>
    public float StepValue => stepValue;

    /// <summary>Engine type to switch to (only used when UpgradeType == EngineUpgrade).</summary>
    public EngineType TargetEngineType => targetEngineType;

    /// <summary>Weapon type to switch to (only used when UpgradeType == WeaponUpgrade).</summary>
    public WeaponType TargetWeaponType => targetWeaponType;
}
