using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Charges and fires a powerful shot.
/// </summary>
public class ChargeShotCommand : ICommand
{
    private readonly WeaponController weapon;
    private readonly float maxChargeTime;
    private readonly float damageMultiplier;

    private float chargeStartTime;
    private bool isCharging;

    public ChargeShotCommand(WeaponController weapon, float maxChargeTime = 2f, float damageMultiplier = 3f)
    {
        this.weapon = weapon;
        this.maxChargeTime = maxChargeTime;
        this.damageMultiplier = damageMultiplier;
    }

    /// <summary>
    /// Whether currently charging.
    /// </summary>
    public bool IsCharging => isCharging;

    /// <summary>
    /// Current charge percentage (0-1).
    /// </summary>
    public float ChargePercent
    {
        get
        {
            if (!isCharging)
            {
                return 0f;
            }

            return Mathf.Clamp01((Time.time - chargeStartTime) / maxChargeTime);
        }
    }

    public bool CanExecute()
    {
        return weapon != null && weapon.CanFire();
    }

    /// <summary>
    /// Begin charging.
    /// </summary>
    public void StartCharge()
    {
        if (!CanExecute())
        {
            return;
        }

        isCharging = true;
        chargeStartTime = Time.time;
    }

    /// <summary>
    /// Release charge and fire.
    /// </summary>
    public void Execute()
    {
        if (!isCharging || weapon == null)
        {
            return;
        }

        float multiplier = 1f + (ChargePercent * (damageMultiplier - 1f));
        weapon.FireCharged(multiplier);

        isCharging = false;
    }

    /// <summary>
    /// Cancel the charge without firing.
    /// </summary>
    public void CancelCharge()
    {
        isCharging = false;
    }
}
