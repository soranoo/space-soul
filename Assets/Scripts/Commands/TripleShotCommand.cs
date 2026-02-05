using UnityEngine;

/// <summary>
/// Fires three projectiles in a spread pattern.
/// </summary>
public class TripleShotCommand : ICommand
{
    private readonly WeaponController weapon;
    private readonly float spreadAngle;

    public TripleShotCommand(WeaponController weapon, float spreadAngle = 15f)
    {
        this.weapon = weapon;
        this.spreadAngle = spreadAngle;
    }

    public bool CanExecute()
    {
        return weapon != null && weapon.CanFire();
    }

    public void Execute()
    {
        if (!CanExecute())
        {
            return;
        }

        weapon.FireSpread(3, spreadAngle);
    }
}
