using System;
using UnityEngine;

/// <summary>
/// Handles enemy spawn positions and timing.
/// Supports multiple spawn patterns (circle, edges, corners, random).
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Area Settings")]
    [Tooltip("Reference to the camera for screen bounds calculation.")]
    [SerializeField] private Camera gameCamera;

    [Tooltip("Viewport scale multiplier: enemies spawn on the boundary of (viewport * scale).")]
    [SerializeField] private float viewportSpawnScale = 1.5f;

    [Header("Safety Settings")]
    [Tooltip("Minimum distance from player for spawning.")]
    [SerializeField] private float minPlayerDistance = 5f;

    [Tooltip("Maximum attempts to find a safe spawn position.")]
    [SerializeField] private int maxSpawnAttempts = 10;

    [Header("Pattern Settings")]
    [Tooltip("Number of positions for circle pattern.")]
    [SerializeField] private int circlePositionCount = 8;

    private Transform playerTransform;
    private int currentCircleIndex;
    private int currentEdgeIndex;
    private int currentCornerIndex;

    /// <summary>
    /// Screen bounds calculated from camera.
    /// </summary>
    public Bounds ScreenBounds { get; private set; }

    private void Start()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        UpdateScreenBounds();
        FindPlayer();
    }

    /// <summary>
    /// Find and cache player reference.
    /// </summary>
    public void FindPlayer()
    {
        GameObject player = GameObject.FindWithTag(TagId.Player.AsString());
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// Update screen bounds based on camera.
    /// </summary>
    public void UpdateScreenBounds()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        if (gameCamera == null)
        {
            // Fallback bounds when no camera
            ScreenBounds = new Bounds(Vector3.zero, new Vector3(20f, 15f, 0f));
            return;
        }

        float height = gameCamera.orthographicSize * 2f;
        float width = height * gameCamera.aspect;

        // Validate for NaN/invalid values
        if (float.IsNaN(height) || float.IsNaN(width) || height <= 0f || width <= 0f)
        {
            Debug.LogWarning("[SpawnManager] Invalid camera bounds, using fallback");
            ScreenBounds = new Bounds(Vector3.zero, new Vector3(20f, 15f, 0f));
            return;
        }

        Vector3 center = gameCamera.transform.position;
        center.z = 0f;

        ScreenBounds = new Bounds(center, new Vector3(width, height, 0f));
    }

    /// <summary>
    /// Get a spawn position based on pattern. All positions are placed on the
    /// boundary of (viewportSpawnScale × camera viewport) — always off-screen.
    /// </summary>
    public Vector3 GetSpawnPosition(SpawnPatternType pattern)
    {
        if (gameCamera == null) gameCamera = Camera.main;
        if (playerTransform == null) FindPlayer();

        Bounds spawnBounds = GetViewportSpawnBounds();

        Vector3 position;
        switch (pattern)
        {
            case SpawnPatternType.Circle:
                position = GetCirclePosition(spawnBounds);
                break;
            case SpawnPatternType.Edges:
                position = GetEdgePosition(spawnBounds);
                break;
            case SpawnPatternType.Corners:
                position = GetCornerPosition(spawnBounds);
                break;
            case SpawnPatternType.Random:
            default:
                position = GetRandomPosition(spawnBounds);
                break;
        }

        if (float.IsNaN(position.x) || float.IsNaN(position.y))
        {
            Debug.LogWarning("[SpawnManager] Invalid position detected, using fallback");
            position = GetRandomPosition(spawnBounds);
        }

        return EnsureSafePosition(position);
    }

    /// <summary>
    /// Returns the spawn boundary: camera viewport scaled by viewportSpawnScale.
    /// Enemies always land on this rectangle's perimeter, guaranteeing off-screen entry.
    /// </summary>
    private Bounds GetViewportSpawnBounds()
    {
        if (gameCamera == null) gameCamera = Camera.main;

        float halfH = (gameCamera != null) ? gameCamera.orthographicSize * viewportSpawnScale : 10f;
        float halfW = (gameCamera != null) ? halfH * gameCamera.aspect : 15f;

        Vector3 center = GetSpawnCenter();
        return new Bounds(center, new Vector3(halfW * 2f, halfH * 2f, 0f));
    }

    /// <summary>
    /// Project a direction angle to the boundary of a rectangle (axis-aligned).
    /// </summary>
    private Vector3 ProjectAngleToRectBoundary(float angle, Bounds bounds)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        float halfW = bounds.extents.x;
        float halfH = bounds.extents.y;

        float tX = (cos != 0f) ? halfW / Mathf.Abs(cos) : float.MaxValue;
        float tY = (sin != 0f) ? halfH / Mathf.Abs(sin) : float.MaxValue;
        float t = Mathf.Min(tX, tY);

        return bounds.center + new Vector3(cos * t, sin * t, 0f);
    }

    /// <summary>
    /// Get a random position on the spawn boundary rectangle.
    /// </summary>
    private Vector3 GetRandomPosition(Bounds spawnBounds)
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return ProjectAngleToRectBoundary(angle, spawnBounds);
    }

    /// <summary>
    /// Distribute enemies evenly around the spawn boundary rectangle.
    /// </summary>
    private Vector3 GetCirclePosition(Bounds spawnBounds)
    {
        float angle = (360f / circlePositionCount) * currentCircleIndex * Mathf.Deg2Rad;
        currentCircleIndex = (currentCircleIndex + 1) % circlePositionCount;
        return ProjectAngleToRectBoundary(angle, spawnBounds);
    }

    /// <summary>
    /// Get a position on one of the four sides of the spawn boundary rectangle.
    /// </summary>
    private Vector3 GetEdgePosition(Bounds spawnBounds)
    {
        float minX = spawnBounds.min.x;
        float maxX = spawnBounds.max.x;
        float minY = spawnBounds.min.y;
        float maxY = spawnBounds.max.y;

        Vector3 position;
        switch (currentEdgeIndex)
        {
            case 0: // Top
                position = new Vector3(UnityEngine.Random.Range(minX, maxX), maxY, 0f);
                break;
            case 1: // Right
                position = new Vector3(maxX, UnityEngine.Random.Range(minY, maxY), 0f);
                break;
            case 2: // Bottom
                position = new Vector3(UnityEngine.Random.Range(minX, maxX), minY, 0f);
                break;
            case 3: // Left
            default:
                position = new Vector3(minX, UnityEngine.Random.Range(minY, maxY), 0f);
                break;
        }

        currentEdgeIndex = (currentEdgeIndex + 1) % 4;
        return position;
    }

    /// <summary>
    /// Get a position at one of the four corners of the spawn boundary rectangle.
    /// </summary>
    private Vector3 GetCornerPosition(Bounds spawnBounds)
    {
        float minX = spawnBounds.min.x;
        float maxX = spawnBounds.max.x;
        float minY = spawnBounds.min.y;
        float maxY = spawnBounds.max.y;

        Vector3 position;
        switch (currentCornerIndex)
        {
            case 0: position = new Vector3(minX, maxY, 0f); break; // Top-left
            case 1: position = new Vector3(maxX, maxY, 0f); break; // Top-right
            case 2: position = new Vector3(maxX, minY, 0f); break; // Bottom-right
            case 3: // Bottom-left
            default: position = new Vector3(minX, minY, 0f); break;
        }

        currentCornerIndex = (currentCornerIndex + 1) % 4;
        return position;
    }

    /// <summary>
    /// Ensure spawn position is safe (not too close to player).
    /// </summary>
    private Vector3 EnsureSafePosition(Vector3 position)
    {
        if (playerTransform == null)
        {
            return position;
        }

        Vector3 playerPosition = playerTransform.position;
        float distanceToPlayer = Vector3.Distance(position, playerPosition);
        if (distanceToPlayer >= minPlayerDistance)
        {
            return position;
        }

        // Try to find a safe position on boundary
        Bounds spawnBounds = GetViewportSpawnBounds();
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 newPosition = GetRandomPosition(spawnBounds);
            if (Vector3.Distance(newPosition, playerPosition) >= minPlayerDistance)
            {
                return newPosition;
            }
        }

        // Fallback: push position away from player
        Vector3 direction = (position - playerPosition).normalized;
        if (direction == Vector3.zero) direction = Vector3.up;
        return playerPosition + direction * minPlayerDistance;
    }

    /// <summary>
    /// Get center point for spawning (camera position).
    /// </summary>
    private Vector3 GetSpawnCenter()
    {
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        if (gameCamera != null)
        {
            Vector3 center = gameCamera.transform.position;
            center.z = 0f;
            return center;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Get a group of spawn positions for circle formation.
    /// </summary>
    /// <param name="count">Number of positions.</param>
    /// <param name="radius">Circle radius.</param>
    /// <returns>Array of positions.</returns>
    public Vector3[] GetCircleFormation(int count, float radius)
    {
        Vector3[] positions = new Vector3[count];
        Vector3 center = GetSpawnCenter();

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i * Mathf.Deg2Rad;
            positions[i] = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                center.y + Mathf.Sin(angle) * radius,
                0f
            );
        }

        return positions;
    }

    /// <summary>
    /// Get a group of spawn positions forming a line.
    /// </summary>
    /// <param name="count">Number of positions.</param>
    /// <param name="start">Start position.</param>
    /// <param name="end">End position.</param>
    /// <returns>Array of positions.</returns>
    public Vector3[] GetLineFormation(int count, Vector3 start, Vector3 end)
    {
        Vector3[] positions = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float t = (count > 1) ? (float)i / (count - 1) : 0.5f;
            positions[i] = Vector3.Lerp(start, end, t);
        }

        return positions;
    }

    /// <summary>
    /// Reset pattern indices.
    /// </summary>
    public void ResetPatterns()
    {
        currentCircleIndex = 0;
        currentEdgeIndex = 0;
        currentCornerIndex = 0;
    }
}
