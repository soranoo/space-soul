using UnityEngine;

/// <summary>
/// Fires a standard projectile from the player ship.
/// </summary>
public class FireWeaponCommand : ICommand
{
    private readonly WeaponController weapon;

    public FireWeaponCommand(WeaponController weapon)
    {
        this.weapon = weapon;
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

        weapon.Fire();
    }
}
