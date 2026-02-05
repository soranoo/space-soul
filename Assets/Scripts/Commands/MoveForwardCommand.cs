using UnityEngine;

/// <summary>
/// Applies forward thrust to the player ship.
/// </summary>
public class MoveForwardCommand : ICommand
{
    private readonly Rigidbody2D rigidbody;
    private readonly PlayerStats stats;

    public MoveForwardCommand(Rigidbody2D rigidbody, PlayerStats stats)
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

        Vector3 force = rigidbody.transform.up * stats.ThrustForce;
        rigidbody.AddForce(force, ForceMode2D.Force);
    }
}
