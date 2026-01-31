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
    /// <param name="speed">Movement speed.</param>
    void UpdateMovement(Transform enemy, float speed);

    /// <summary>
    /// Initialize the movement pattern with any required references.
    /// </summary>
    /// <param name="enemy">The enemy transform.</param>
    void Initialize(Transform enemy);

    /// <summary>
    /// Reset the pattern state.
    /// </summary>
    void Reset();
}
