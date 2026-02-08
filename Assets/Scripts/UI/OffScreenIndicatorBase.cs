using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Abstract base for poolable UI indicators that clamp to the screen edge
/// and rotate to point toward a tracked world-space target.
/// Subclasses provide the target position and handle any extra visuals.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public abstract class OffScreenIndicatorBase : MonoBehaviour, IPoolable
{
    private string poolIdOverride;

    [Header("Rotation")]
    [Tooltip("Extra rotation offset in degrees. Use -90 if the sprite points up.")]
    [SerializeField] private float rotationOffset = -90f;

    [Header("Pop Animation")]
    [SerializeField] private float popScaleFrom = 0f;
    [SerializeField] private float popScaleTo = 1f;
    [SerializeField] private float popScaleOut = 0f;
    [SerializeField] private float popInDuration = 0.15f;
    [SerializeField] private float popOutDuration = 0.12f;
    [SerializeField] private Ease popInEase = Ease.OutBack;
    [SerializeField] private Ease popOutEase = Ease.InBack;

    protected RectTransform rectTransform;
    protected float lastAngleDeg;
    private Tween scaleTween;
    private bool isDespawning;

    public bool IsDespawning => isDespawning;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Returns the world-space position of the tracked target, or null if no target.
    /// </summary>
    protected abstract Vector3? GetTrackedWorldPosition();

    /// <summary>
    /// Called after the indicator has been positioned and rotated on the screen edge.
    /// Override to apply extra logic such as counter-rotating child elements.
    /// </summary>
    protected virtual void OnPositioned(float angleDeg) { }

    /// <summary>
    /// Called each frame before the position/visibility check.
    /// Override to update per-frame visuals such as fill amount.
    /// </summary>
    protected virtual void OnBeforeUpdate() { }

    /// <summary>
    /// Called by subclasses or a manager each frame to update this indicator.
    /// </summary>
    public void UpdateIndicator(Camera cam, RectTransform canvasRect, float edgePadding)
    {
        if (isDespawning)
        {
            return;
        }

        Vector3? targetPos = GetTrackedWorldPosition();
        if (targetPos == null || cam == null || canvasRect == null)
        {
            return;
        }

        OnBeforeUpdate();

        Vector3 screenPos = cam.WorldToScreenPoint(targetPos.Value);

        bool onScreen = screenPos.z > 0f
            && screenPos.x > 0f && screenPos.x < Screen.width
            && screenPos.y > 0f && screenPos.y < Screen.height;

        if (onScreen)
        {
            return;
        }

        // If behind camera, flip
        if (screenPos.z < 0f)
        {
            screenPos *= -1f;
        }

        // Center-relative direction
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 dir = ((Vector2)screenPos - screenCenter).normalized;

        // Clamp to screen edge rectangle
        float halfW = screenCenter.x - edgePadding;
        float halfH = screenCenter.y - edgePadding;

        float tX = Mathf.Abs(dir.x) > 0.0001f ? halfW / Mathf.Abs(dir.x) : float.MaxValue;
        float tY = Mathf.Abs(dir.y) > 0.0001f ? halfH / Mathf.Abs(dir.y) : float.MaxValue;
        float t = Mathf.Min(tX, tY);

        Vector2 edgePos = screenCenter + dir * t;

        // Convert to canvas local position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, edgePos, null, out Vector2 localPoint);

        rectTransform.localPosition = localPoint;

        // Rotate to point toward target
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
        lastAngleDeg = angle;

        OnPositioned(angle);
    }

    public static bool IsOnScreen(Camera cam, Vector3 worldPos)
    {
        if (cam == null)
        {
            return false;
        }

        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        return screenPos.z > 0f
            && screenPos.x > 0f && screenPos.x < Screen.width
            && screenPos.y > 0f && screenPos.y < Screen.height;
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

    public virtual void OnSpawn()
    {
        isDespawning = false;
        scaleTween?.Kill();

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * popScaleFrom;

            if (popInDuration > 0f)
            {
                scaleTween = rectTransform.DOScale(popScaleTo, popInDuration).SetEase(popInEase);
            }
            else
            {
                rectTransform.localScale = Vector3.one * popScaleTo;
            }
        }

        gameObject.SetActive(true);
    }

    public virtual void OnDespawn()
    {
        scaleTween?.Kill();
        isDespawning = false;
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one * popScaleTo;
        }

        gameObject.SetActive(false);
    }

    public void PlayDespawn(Action onComplete)
    {
        if (isDespawning)
        {
            return;
        }

        isDespawning = true;
        scaleTween?.Kill();

        if (rectTransform == null || popOutDuration <= 0f)
        {
            isDespawning = false;
            onComplete?.Invoke();
            return;
        }

        scaleTween = rectTransform.DOScale(popScaleOut, popOutDuration)
            .SetEase(popOutEase)
            .OnComplete(() =>
            {
                isDespawning = false;
                onComplete?.Invoke();
            });
    }

    #endregion
}
