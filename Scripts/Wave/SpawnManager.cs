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

    [Tooltip("Padding from screen edges for edge spawning.")]
    [SerializeField] private float edgePadding = 1f;

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
        GameObject player = GameObject.FindWithTag("Player");
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
    /// Get a spawn position based on pattern and distance settings.
    /// </summary>
    /// <param name="pattern">Spawn pattern type.</param>
    /// <param name="minDistance">Minimum spawn distance from center.</param>
    /// <param name="maxDistance">Maximum spawn distance from center.</param>
    /// <returns>Safe spawn position.</returns>
    public Vector3 GetSpawnPosition(SpawnPatternType pattern, float minDistance, float maxDistance)
    {
        // Ensure camera is valid
        if (gameCamera == null)
        {
            gameCamera = Camera.main;
        }

        // Refresh player reference if needed
        if (playerTransform == null)
        {
            FindPlayer();
        }

        // Validate distances
        if (minDistance <= 0f) minDistance = 5f;
        if (maxDistance <= minDistance) maxDistance = minDistance + 5f;

        Vector3 position;

        switch (pattern)
        {
            case SpawnPatternType.Circle:
                position = GetCirclePosition(minDistance, maxDistance);
                break;

            case SpawnPatternType.Edges:
                position = GetEdgePosition();
                break;

            case SpawnPatternType.Corners:
                position = GetCornerPosition();
                break;

            case SpawnPatternType.Random:
            default:
                position = GetRandomPosition(minDistance, maxDistance);
                break;
        }

        // Validate position for NaN
        if (float.IsNaN(position.x) || float.IsNaN(position.y))
        {
            Debug.LogWarning("[SpawnManager] Invalid position detected, using fallback");
            position = GetRandomPosition(minDistance, maxDistance);
        }

        return EnsureSafePosition(position, minDistance, maxDistance);
    }

    /// <summary>
    /// Get a random position within spawn distance.
    /// </summary>
    private Vector3 GetRandomPosition(float minDistance, float maxDistance)
    {
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = UnityEngine.Random.Range(minDistance, maxDistance);

        Vector3 center = GetSpawnCenter();
        return new Vector3(
            center.x + Mathf.Cos(angle) * distance,
            center.y + Mathf.Sin(angle) * distance,
            0f
        );
    }

    /// <summary>
    /// Get a position on a circle around center.
    /// Distributes enemies evenly around the circle.
    /// </summary>
    private Vector3 GetCirclePosition(float minDistance, float maxDistance)
    {
        float angle = (360f / circlePositionCount) * currentCircleIndex * Mathf.Deg2Rad;
        float distance = (minDistance + maxDistance) / 2f;

        currentCircleIndex = (currentCircleIndex + 1) % circlePositionCount;

        Vector3 center = GetSpawnCenter();
        return new Vector3(
            center.x + Mathf.Cos(angle) * distance,
            center.y + Mathf.Sin(angle) * distance,
            0f
        );
    }

    /// <summary>
    /// Get a position on screen edges.
    /// </summary>
    private Vector3 GetEdgePosition()
    {
        UpdateScreenBounds();

        float minX = ScreenBounds.min.x - edgePadding;
        float maxX = ScreenBounds.max.x + edgePadding;
        float minY = ScreenBounds.min.y - edgePadding;
        float maxY = ScreenBounds.max.y + edgePadding;

        Vector3 position;

        // Cycle through edges: 0=top, 1=right, 2=bottom, 3=left
        switch (currentEdgeIndex)
        {
            case 0: // Top
                position = new Vector3(
                    UnityEngine.Random.Range(minX, maxX),
                    maxY,
                    0f
                );
                break;

            case 1: // Right
                position = new Vector3(
                    maxX,
                    UnityEngine.Random.Range(minY, maxY),
                    0f
                );
                break;

            case 2: // Bottom
                position = new Vector3(
                    UnityEngine.Random.Range(minX, maxX),
                    minY,
                    0f
                );
                break;

            case 3: // Left
            default:
                position = new Vector3(
                    minX,
                    UnityEngine.Random.Range(minY, maxY),
                    0f
                );
                break;
        }

        currentEdgeIndex = (currentEdgeIndex + 1) % 4;
        return position;
    }

    /// <summary>
    /// Get a position at screen corners.
    /// </summary>
    private Vector3 GetCornerPosition()
    {
        UpdateScreenBounds();

        float minX = ScreenBounds.min.x - edgePadding;
        float maxX = ScreenBounds.max.x + edgePadding;
        float minY = ScreenBounds.min.y - edgePadding;
        float maxY = ScreenBounds.max.y + edgePadding;

        Vector3 position;

        // Cycle through corners: 0=top-left, 1=top-right, 2=bottom-right, 3=bottom-left
        switch (currentCornerIndex)
        {
            case 0: // Top-left
                position = new Vector3(minX, maxY, 0f);
                break;

            case 1: // Top-right
                position = new Vector3(maxX, maxY, 0f);
                break;

            case 2: // Bottom-right
                position = new Vector3(maxX, minY, 0f);
                break;

            case 3: // Bottom-left
            default:
                position = new Vector3(minX, minY, 0f);
                break;
        }

        currentCornerIndex = (currentCornerIndex + 1) % 4;
        return position;
    }

    /// <summary>
    /// Ensure spawn position is safe (not too close to player).
    /// </summary>
    private Vector3 EnsureSafePosition(Vector3 position, float minDistance, float maxDistance)
    {
        if (playerTransform == null)
        {
            return position;
        }

        Vector3 playerPosition = playerTransform.position;

        // Check if position is too close to player
        float distanceToPlayer = Vector3.Distance(position, playerPosition);
        if (distanceToPlayer >= minPlayerDistance)
        {
            return position;
        }

        // Try to find a safe position
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 newPosition = GetRandomPosition(minDistance, maxDistance);
            distanceToPlayer = Vector3.Distance(newPosition, playerPosition);

            if (distanceToPlayer >= minPlayerDistance)
            {
                return newPosition;
            }
        }

        // Fallback: push position away from player
        Vector3 direction = (position - playerPosition).normalized;
        if (direction == Vector3.zero)
        {
            direction = Vector3.up;
        }

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
