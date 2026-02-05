/// <summary>
/// Grants a temporary shield that absorbs damage.
/// </summary>
public class ShieldPowerUp : IPowerUp
{
    private readonly float duration;
    private readonly int shieldAmount;
    private readonly PowerUpController controller;

    public ShieldPowerUp(float duration, int shieldAmount, PowerUpController controller)
    {
        this.duration = duration;
        this.shieldAmount = shieldAmount;
        this.controller = controller;
    }

    public void Activate(PlayerController player)
    {
        controller?.AddShield(shieldAmount);
    }

    public void Deactivate(PlayerController player)
    {
        controller?.ClearShield();
    }

    public float GetDuration()
    {
        return duration;
    }
}
