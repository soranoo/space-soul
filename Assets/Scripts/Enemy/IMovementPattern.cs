using UnityEngine;

/// <summary>
/// Defines enemy movement behavior.
/// Implements Strategy pattern for interchangeable movement.
/// </summary>
public interface IMovementPattern
{
    /// <summary>
    /// Update the enemy's position based on this movement pattern.
    /// </summary>
    /// <param name="enemy">The enemy transform to move.</param>
    /// <param name="rb">The enemy rigidbody used for movement.</param>
    /// <param name="speed">Movement speed.</param>
    void UpdateMovement(Transform enemy, Rigidbody2D rb, float speed);

    /// <summary>
    /// Initialize the movement pattern with any required references.
    /// </summary>
    /// <param name="enemy">The enemy transform.</param>
    /// <param name="rb">The enemy rigidbody.</param>
    void Initialize(Transform enemy, Rigidbody2D rb);

    /// <summary>
    /// Reset the pattern state.
    /// </summary>
    void Reset();
}
