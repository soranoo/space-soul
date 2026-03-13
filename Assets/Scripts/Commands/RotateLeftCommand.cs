using UnityEngine;

/// <summary>
/// Rotates the player ship counter-clockwise.
/// </summary>
public class RotateLeftCommand : ICommand
{
    private readonly Rigidbody2D rigidbody;
    private readonly PlayerController player;

    public RotateLeftCommand(Rigidbody2D rigidbody, PlayerController player)
    {
        this.rigidbody = rigidbody;
        this.player = player;
    }

    public bool CanExecute()
    {
        return rigidbody != null && player != null;
    }

    public void Execute()
    {
        if (!CanExecute())
        {
            return;
        }

        rigidbody.AddTorque(player.GetCurrentRotationSpeed());
    }
}
