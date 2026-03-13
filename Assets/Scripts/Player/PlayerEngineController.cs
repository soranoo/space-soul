using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player engine visuals and powering animation based on input.
/// Attach to the engine pivot.
/// </summary>
public class PlayerEngineController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputHandler inputHandler;

    [Header("Engine Prefabs")]
    [SerializeField] private PlayerEngine baseEnginePrefab;
    [SerializeField] private PlayerEngine bigPulseEnginePrefab;
    [SerializeField] private PlayerEngine burstEnginePrefab;
    [SerializeField] private PlayerEngine superchargedEnginePrefab;

    [Header("Settings")]
    [SerializeField] private EngineType defaultEngineType = EngineType.Base;
    private readonly Dictionary<EngineType, GameObject> spawnedEngines = new Dictionary<EngineType, GameObject>();
    private readonly Dictionary<EngineType, PlayerEngine> spawnedEngineControllers = new Dictionary<EngineType, PlayerEngine>();
    private EngineType currentEngineType;
    private PlayerEngine currentEngine;

    private void Awake()
    {
        if (inputHandler == null)
        {
            inputHandler = GetComponentInParent<InputHandler>();
        }

        // Spawn all engine types at start to avoid runtime instantiation
        SpawnAllEngines();
        SetEngineType(defaultEngineType);
    }

    private void Update()
    {
        bool isPowering = inputHandler != null && inputHandler.IsThrusting;
        currentEngine?.SetPowering(isPowering);
    }

    /// <summary>
    /// Switch to a new engine visual type.
    /// </summary>
    public void SetEngineType(EngineType engineType)
    {
        if (currentEngineType == engineType && currentEngine != null)
        {
            return;
        }

        DeactivateCurrentEngine();

        GameObject engineInstance = GetEngine(engineType);
        if (engineInstance != null)
        {
            engineInstance.SetActive(true);
            currentEngine = GetEngineController(engineType);
        }
        else
        {
            currentEngine = null;
        }

        currentEngineType = engineType;
    }

    public EngineType CurrentEngineType => currentEngineType;

    public PlayerEngine CurrentEngine => currentEngine;

    private void SpawnAllEngines()
    {
        SpawnEngine(EngineType.Base, baseEnginePrefab.gameObject);
        SpawnEngine(EngineType.BigPulse, bigPulseEnginePrefab.gameObject);
        SpawnEngine(EngineType.Burst, burstEnginePrefab.gameObject);
        SpawnEngine(EngineType.Supercharged, superchargedEnginePrefab.gameObject);
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

        PlayerEngine engineController = instance.GetComponent<PlayerEngine>();
        if (engineController == null)
        {
            engineController = instance.AddComponent<PlayerEngine>();
        }


        spawnedEngines[engineType] = instance;
        spawnedEngineControllers[engineType] = engineController;
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
        currentEngine?.SetPowering(false);

        if (spawnedEngines.TryGetValue(currentEngineType, out GameObject existing) && existing != null)
        {
            existing.SetActive(false);
        }
    }

    private PlayerEngine GetEngineController(EngineType engineType)
    {
        if (spawnedEngineControllers.TryGetValue(engineType, out PlayerEngine controller) && controller != null)
        {
            return controller;
        }

        return null;
    }
}
