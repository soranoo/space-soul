using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A poolable UI indicator that points toward an off-screen power-up pickup.
/// Displays the power-up icon and uses Image.fillAmount to show remaining despawn time.
/// Inherits edge-clamping and pooling from OffScreenIndicatorBase.
/// </summary>
public class PowerUpOffScreenIndicator : OffScreenIndicatorBase
{
    [Header("UI References")]
    [Tooltip("Filled image that shows the power-up icon. Type should be set to Filled.")]
    [SerializeField] private Image iconFillImage;

    [Header("Icon Rotation")]
    [Tooltip("If true, the icon counter-rotates so it stays upright while the root rotates.")]
    [SerializeField] private bool keepIconUpright = true;

    private RectTransform iconRect;
    private PowerUpPickup trackedPickup;

    /// <summary>
    /// The pickup this indicator currently tracks.
    /// </summary>
    public PowerUpPickup TrackedPickup => trackedPickup;

    protected override void Awake()
    {
        base.Awake();

        if (iconFillImage != null)
        {
            iconRect = iconFillImage.GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// Assign the pickup to track and copy its icon.
    /// </summary>
    public void SetTarget(PowerUpPickup pickup)
    {
        trackedPickup = pickup;

        if (iconFillImage != null && pickup != null && pickup.PowerUpDataRef != null)
        {
            iconFillImage.fillAmount = 1f;
            iconFillImage.enabled = true;
        }
    }

    protected override Vector3? GetTrackedWorldPosition()
    {
        if (trackedPickup == null)
        {
            return null;
        }

        return trackedPickup.transform.position;
    }

    protected override void OnBeforeUpdate()
    {
        // Update fill amount based on remaining despawn time
        if (iconFillImage != null && trackedPickup != null)
        {
            iconFillImage.fillAmount = trackedPickup.RemainingNormalized;
        }
    }

    protected override void OnPositioned(float angleDeg)
    {
        // Counter-rotate the icon so it stays upright
        if (keepIconUpright && iconRect != null)
        {
            iconRect.localRotation = Quaternion.Euler(0f, 0f, -angleDeg);
        }
    }

    public override void OnDespawn()
    {
        trackedPickup = null;
        iconFillImage.fillAmount = 1f;
        base.OnDespawn();
    }
}
