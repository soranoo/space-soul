/// <summary>
/// Increases player fire rate temporarily.
/// </summary>
public class RapidFirePowerUp : IPowerUp
{
    private readonly float duration;
    private readonly float multiplier;

    public RapidFirePowerUp(float duration, float multiplier)
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

        player.Stats.ModifyFireRate(multiplier);
    }

    public void Deactivate(PlayerController player)
    {
        if (player == null)
        {
            return;
        }

        float inverse = multiplier != 0f ? 1f / multiplier : 1f;
        player.Stats.ModifyFireRate(inverse);
    }

    public float GetDuration()
    {
        return duration;
    }
}
