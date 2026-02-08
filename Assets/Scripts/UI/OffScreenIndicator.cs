using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A poolable UI indicator arrow that points toward an off-screen enemy.
/// Attach to a prefab containing a RectTransform with an arrow/pointer Image.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class OffScreenIndicator : MonoBehaviour, IPoolable
{
    [SerializeField] private string poolIdOverride;
    [SerializeField] private Image arrowImage;

    [Header("Rotation")]
    [Tooltip("Extra rotation offset in degrees. Use -90 if the arrow sprite points up.")]
    [SerializeField] private float rotationOffset = -90f;

    private RectTransform rectTransform;
    private Transform trackedEnemy;
    private float lastAngleDeg;

    /// <summary>
    /// The enemy this indicator is currently tracking.
    /// </summary>
    public Transform TrackedEnemy => trackedEnemy;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Assign the enemy transform to track.
    /// </summary>
    public void SetTarget(Transform enemy)
    {
        trackedEnemy = enemy;
    }

    /// <summary>
    /// Update position and rotation on the screen edge.
    /// Called externally by EnemyIndicatorUI each frame.
    /// </summary>
    public void UpdateIndicator(Camera cam, RectTransform canvasRect, float edgePadding)
    {
        if (trackedEnemy == null || cam == null || canvasRect == null)
        {
            return;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(trackedEnemy.position);

        // If on-screen, hide
        bool onScreen = screenPos.z > 0f
            && screenPos.x > 0f && screenPos.x < Screen.width
            && screenPos.y > 0f && screenPos.y < Screen.height;

        if (arrowImage != null)
        {
            arrowImage.enabled = !onScreen;
        }

        if (onScreen)
        {
            return;
        }

        // If behind camera, flip
        if (screenPos.z < 0f)
        {
            screenPos *= -1f;
        }

        // Center-relative screen position
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = ((Vector2)screenPos - screenCenter).normalized;

        // Clamp to screen edges with padding
        float halfW = screenCenter.x - edgePadding;
        float halfH = screenCenter.y - edgePadding;

        // Find intersection with screen edge rectangle
        float tX = Mathf.Abs(dir.x) > 0.0001f ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
        float tY = Mathf.Abs(dir.y) > 0.0001f ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
        float t = Mathf.Min(tX, tY);

        Vector2 edgePos = screenCenter + dir * t;

        // Convert screen position to canvas local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, edgePos, null, out Vector2 localPoint);

        rectTransform.localPosition = localPoint;

        // Rotate arrow to point toward enemy
        // Atan2 gives 0° = right; offset so the sprite's natural "up" points correctly
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        lastAngleDeg = angle;
    }

    #region IPoolable

    public string GetPoolId()
    {
        if (!string.IsNullOrWhiteSpace(poolIdOverride))
        {
            return poolIdOverride;
        }

        return gameObject.name;
    }

    public void SetPoolId(string poolId)
    {
        poolIdOverride = poolId;
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);

        if (arrowImage != null)
        {
            arrowImage.enabled = true;
        }
    }

    public void OnDespawn()
    {
        trackedEnemy = null;

        if (arrowImage != null)
        {
            arrowImage.enabled = false;
        }

        gameObject.SetActive(false);
    }

    #endregion
}
