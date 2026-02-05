using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player engine visuals and powering animation based on movement.
/// Attach to the engine pivot.
/// </summary>
public class PlayerEngineController : MonoBehaviour
{
    private const string ANIM_BOOL_POWERING = "Powering";

    [Header("References")]
    [SerializeField] private Rigidbody2D playerRigidbody;

    [Header("Engine Prefabs")]
    [SerializeField] private GameObject baseEnginePrefab;
    [SerializeField] private GameObject bigPulseEnginePrefab;
    [SerializeField] private GameObject burstEnginePrefab;
    [SerializeField] private GameObject superchargedEnginePrefab;

    [Header("Settings")]
    [SerializeField] private EngineType defaultEngineType = EngineType.Base;
    [SerializeField] private float moveThreshold = 0.05f;

    private readonly Dictionary<EngineType, GameObject> spawnedEngines = new Dictionary<EngineType, GameObject>();
    private EngineType currentEngineType;
    private Animator currentAnimator;

    private void Awake()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponentInParent<Rigidbody2D>();
        }

        // Spawn all engine types at start to avoid runtime instantiation
        SpawnAllEngines();
        SetEngineType(defaultEngineType);
    }

    private void Update()
    {
        bool isMoving = false;
        if (playerRigidbody != null)
        {
            isMoving = playerRigidbody.linearVelocity.sqrMagnitude > moveThreshold * moveThreshold;
        }

        if (currentAnimator != null)
        {
            currentAnimator.SetBool(ANIM_BOOL_POWERING, isMoving);
        }
    }

    /// <summary>
    /// Switch to a new engine visual type.
    /// </summary>
    public void SetEngineType(EngineType engineType)
    {
        if (currentEngineType == engineType && currentAnimator != null)
        {
            return;
        }

        DeactivateCurrentEngine();

        GameObject engineInstance = GetEngine(engineType);
        if (engineInstance != null)
        {
            engineInstance.SetActive(true);
            currentAnimator = engineInstance.GetComponentInChildren<Animator>();
        }
        else
        {
            currentAnimator = null;
        }

        currentEngineType = engineType;
    }

    public EngineType CurrentEngineType => currentEngineType;

    private void SpawnAllEngines()
    {
        SpawnEngine(EngineType.Base, baseEnginePrefab);
        SpawnEngine(EngineType.BigPulse, bigPulseEnginePrefab);
        SpawnEngine(EngineType.Burst, burstEnginePrefab);
        SpawnEngine(EngineType.Supercharged, superchargedEnginePrefab);
    }

    private void SpawnEngine(EngineType engineType, GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        if (spawnedEngines.ContainsKey(engineType) && spawnedEngines[engineType] != null)
        {
            return;
        }

        GameObject instance = Instantiate(prefab, transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.SetActive(false);

        spawnedEngines[engineType] = instance;
    }

    private GameObject GetEngine(EngineType engineType)
    {
        if (spawnedEngines.TryGetValue(engineType, out GameObject existing) && existing != null)
        {
            return existing;
        }

        return null;
    }

    private void DeactivateCurrentEngine()
    {
        if (spawnedEngines.TryGetValue(currentEngineType, out GameObject existing) && existing != null)
        {
            existing.SetActive(false);
        }
    }

    private GameObject GetPrefabForType(EngineType engineType)
    {
        switch (engineType)
        {
            case EngineType.Base:
                return baseEnginePrefab;
            case EngineType.BigPulse:
                return bigPulseEnginePrefab;
            case EngineType.Burst:
                return burstEnginePrefab;
            case EngineType.Supercharged:
                return superchargedEnginePrefab;
            default:
                return baseEnginePrefab;
        }
    }
}
