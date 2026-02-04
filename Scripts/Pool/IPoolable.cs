/// <summary>
/// Contract for objects that can be pooled.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Returns the pool identifier for this prefab type.
    /// </summary>
    /// <returns>Stable pool id for this prefab.</returns>
    string GetPoolId();

    /// <summary>
    /// Assigns the pool identifier to this instance.
    /// </summary>
    /// <param name="poolId">Stable pool id for this prefab.</param>
    void SetPoolId(string poolId);

    /// <summary>
    /// Called when retrieved from pool.
    /// Use to reset state and enable the object.
    /// </summary>
    void OnSpawn();

    /// <summary>
    /// Called when returned to pool.
    /// Use to disable the object and clean up.
    /// </summary>
    void OnDespawn();
}
