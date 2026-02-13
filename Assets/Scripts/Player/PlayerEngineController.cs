using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player engine visuals and powering animation based on input.
/// Attach to the engine pivot.
/// </summary>
public class PlayerEngineController : MonoBehaviour
{
    private const string ANIM_BOOL_POWERING = "Powering";

    [Header("References")]
    [SerializeField] private InputHandler inputHandler;

    [Header("Engine Audio")]
    [SerializeField] private AudioSettings engineSfx;
    [SerializeField] private AudioSource engineAudioSource;

    [Header("Engine Prefabs")]
    [SerializeField] private GameObject baseEnginePrefab;
    [SerializeField] private GameObject bigPulseEnginePrefab;
    [SerializeField] private GameObject burstEnginePrefab;
    [SerializeField] private GameObject superchargedEnginePrefab;

    [Header("Settings")]
    [SerializeField] private EngineType defaultEngineType = EngineType.Base;
    private readonly Dictionary<EngineType, GameObject> spawnedEngines = new Dictionary<EngineType, GameObject>();
    private EngineType currentEngineType;
    private Animator currentAnimator;

    private void Awake()
    {
        if (inputHandler == null)
        {
            inputHandler = GetComponentInParent<InputHandler>();
        }

        if (engineAudioSource == null)
        {
            engineAudioSource = GetComponent<AudioSource>();

            if (engineAudioSource == null)
            {
                engineAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        ConfigureEngineAudioSource();

        // Spawn all engine types at start to avoid runtime instantiation
        SpawnAllEngines();
        SetEngineType(defaultEngineType);
    }

    private void Update()
    {
        bool isPowering = inputHandler != null && inputHandler.IsThrusting;

        if (currentAnimator != null)
        {
            currentAnimator.SetBool(ANIM_BOOL_POWERING, isPowering);
        }

        UpdateEngineAudio(isPowering);
    }

    private void OnDisable()
    {
        if (engineAudioSource != null && engineAudioSource.isPlaying)
        {
            engineAudioSource.Stop();
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

    private void ConfigureEngineAudioSource()
    {
        if (engineAudioSource == null)
        {
            return;
        }

        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = true;

        if (engineSfx != null)
        {
            engineAudioSource.clip = engineSfx.Clip;
            engineSfx.Source?.ApplyTo(engineAudioSource);
            engineAudioSource.loop = true;
        }
    }

    private void UpdateEngineAudio(bool isPowering)
    {
        if (engineAudioSource == null)
        {
            return;
        }

        if (engineSfx == null || engineSfx.Clip == null)
        {
            if (engineAudioSource.isPlaying)
            {
                engineAudioSource.Stop();
            }

            return;
        }

        if (isPowering)
        {
            if (engineAudioSource.clip != engineSfx.Clip)
            {
                engineAudioSource.clip = engineSfx.Clip;
            }

            if (!engineAudioSource.isPlaying)
            {
                engineSfx.Source?.ApplyTo(engineAudioSource);
                engineAudioSource.loop = true;
                engineAudioSource.Play();
            }
        }
        else if (engineAudioSource.isPlaying)
        {
            engineAudioSource.Stop();
        }
    }
}
