using UnityEngine;

/// <summary>
/// Movement pattern that chases the player directly.
/// </summary>
public class ChasePlayerPattern : IMovementPattern
{
    private Transform enemyTransform;
    private Rigidbody2D enemyRb;
    private Transform playerTransform;

    /// <summary>
    /// Initialize the movement pattern.
    /// </summary>
    /// <param name="enemy">The enemy transform.</param>
    /// <param name="rb">The enemy rigidbody.</param>
    public void Initialize(Transform enemy, Rigidbody2D rb)
    {
        enemyTransform = enemy;
        enemyRb = rb;
        FindPlayer();
    }

    /// <summary>
    /// Find the player in the scene.
    /// </summary>
    private void FindPlayer()
    {
        PlayerController player = Object.FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// Update movement toward player.
    /// </summary>
    /// <param name="enemy">The enemy transform to move.</param>
    /// <param name="rb">The enemy rigidbody used for movement.</param>
    /// <param name="speed">Movement speed.</param>
    public void UpdateMovement(Transform enemy, Rigidbody2D rb, float speed)
    {
        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null)
            {
                return;
            }
        }

        Vector2 direction = ((Vector2)playerTransform.position - (Vector2)enemy.position).normalized;

        Rigidbody2D targetBody = rb != null ? rb : enemyRb;
        if (targetBody != null)
        {
            Vector2 nextPosition = targetBody.position + direction * speed * Time.deltaTime;
            targetBody.MovePosition(nextPosition);
        }
        else
        {
            enemy.Translate(direction * speed * Time.deltaTime, Space.World);
        }

        // Rotate to face player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        enemy.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Reset the pattern state.
    /// </summary>
    public void Reset()
    {
        enemyRb = null;
        playerTransform = null;
    }
}
