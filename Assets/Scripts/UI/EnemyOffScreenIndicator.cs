using UnityEngine;
/// <summary>
/// A poolable UI indicator arrow that points toward an off-screen enemy.
/// Inherits edge-clamping and pooling from OffScreenIndicatorBase.
/// </summary>
public class EnemyOffScreenIndicator : OffScreenIndicatorBase
{
    private Transform trackedTarget;

    /// <summary>
    /// The transform this indicator is currently tracking.
    /// </summary>
    public Transform TrackedTarget => trackedTarget;

    /// <summary>
    /// Assign the transform to track.
    /// </summary>
    public void SetTarget(Transform target)
    {
        trackedTarget = target;
    }

    protected override Vector3? GetTrackedWorldPosition()
    {
        if (trackedTarget == null)
        {
            return null;
        }

        return trackedTarget.position;
    }

    public override void OnDespawn()
    {
        trackedTarget = null;
        base.OnDespawn();
    }
}
