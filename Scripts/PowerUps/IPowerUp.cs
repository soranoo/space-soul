/// <summary>
/// Contract for all power-up effects.
/// </summary>
public interface IPowerUp
{
    /// <summary>
    /// Apply the effect to the player.
    /// </summary>
    void Activate(PlayerController player);

    /// <summary>
    /// Remove the effect from the player.
    /// </summary>
    void Deactivate(PlayerController player);

    /// <summary>
    /// Duration of the effect in seconds.
    /// </summary>
    float GetDuration();
}
