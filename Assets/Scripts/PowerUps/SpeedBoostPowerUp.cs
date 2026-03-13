/// <summary>
/// Increases player movement speed temporarily.
/// </summary>
public class SpeedBoostPowerUp : IPowerUp
{
    private readonly float duration;
    private readonly float multiplier;

    public SpeedBoostPowerUp(float duration, float multiplier)
    {
        this.duration = duration;
        this.multiplier = multiplier;
    }

    public void Activate(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        PlayerStats stats = player.Stats;
        stats.ModifyThrustForceMultiplier(multiplier);
        stats.ModifyRotationSpeedMultiplier(multiplier);
        stats.ModifyMaxSpeedMultiplier(multiplier);
    }

    public void Deactivate(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        float inverse = multiplier != 0f ? 1f / multiplier : 1f;
        PlayerStats stats = player.Stats;
        stats.ModifyThrustForceMultiplier(inverse);
        stats.ModifyRotationSpeedMultiplier(inverse);
        stats.ModifyMaxSpeedMultiplier(inverse);
    }

    public float GetDuration()
    {
        return duration;
    }
}
