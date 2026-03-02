using UnityEngine;

/// <summary>
/// Blocks enemy attacks that hit the shield.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShieldBlocker : MonoBehaviour
{
    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EnemyProjectile projectile = other.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            PoolManager.Instance.Release(projectile);
        }
    }
}
