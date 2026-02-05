using UnityEngine;

/// <summary>
/// Rotates the player ship counter-clockwise.
/// </summary>
public class RotateLeftCommand : ICommand
{
    private readonly Rigidbody2D rigidbody;
    private readonly PlayerStats stats;

    public RotateLeftCommand(Rigidbody2D rigidbody, PlayerStats stats)
    {
        this.rigidbody = rigidbody;
        this.stats = stats;
    }

    public bool CanExecute()
    {
        return rigidbody != null && stats != null;
    }

    public void Execute()
    {
        if (!CanExecute())
        {
            return;
        }

        rigidbody.AddTorque(stats.RotationSpeed);
    }
}
