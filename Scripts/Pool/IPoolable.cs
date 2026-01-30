/// <summary>
/// Contract for objects that can be pooled.
/// </summary>
public interface IPoolable
{
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
