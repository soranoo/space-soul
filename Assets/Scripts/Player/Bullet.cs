using UnityEngine;

/// <summary>
/// Basic bullet behavior.
/// Implements IPoolable for object pooling support.
/// </summary>
public class Bullet : ProjectileBase
{
    [Header("Settings")]
    [SerializeField] private float speed = 15f;
    [SerializeField] private int baseDamage = 1;

    private float damageMultiplier = 1f;

    /// <summary>
    /// Set the damage multiplier for this bullet.
    /// </summary>
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = multiplier;
    }

    /// <summary>
    /// Get the effective damage.
    /// </summary>
    public int GetDamage()
    {
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    public override void OnSpawn()
    {
        base.OnSpawn();
        damageMultiplier = 1f;
    }

    protected override void Update()
    {
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        base.Update();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(TagId.Enemy.AsString()))
        {
            PlayHitSfx();
            DespawnSelf();
        }
    }
}
