using UnityEngine;

/// <summary>
/// Rotates the player ship clockwise.
/// </summary>
public class RotateRightCommand : ICommand
{
    private readonly Rigidbody2D rigidbody;
    private readonly PlayerStats stats;

    public RotateRightCommand(Rigidbody2D rigidbody, PlayerStats stats)
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

        rigidbody.AddTorque(-stats.RotationSpeed);
    }
}
