using UnityEngine;

/// <summary>
/// Applies forward thrust to the player ship.
/// </summary>
public class MoveForwardCommand : ICommand
{
    private readonly Rigidbody2D rigidbody;
    private readonly PlayerController player;

    public MoveForwardCommand(Rigidbody2D rigidbody, PlayerController player)
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

        Vector3 force = rigidbody.transform.up * player.GetCurrentThrustForce();
        rigidbody.AddForce(force, ForceMode2D.Force);
    }
}
