using UnityEngine;

/// <summary>
/// Handles enemy engine visual/audio behavior.
/// Engine turns on only while the enemy is moving.
/// </summary>
[DisallowMultipleComponent]
public class EnemyEngine : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject engineVisual;

    [Header("Audio")]
    [SerializeField] private AudioSettings engineSfx;
    [SerializeField] private AudioSource engineAudioSource;

    private bool forceDisabled;
    private bool movementCommandedThisFrame;
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        if (engineAudioSource == null)
        {
            engineAudioSource = GetComponent<AudioSource>();
            if (engineAudioSource == null)
            {
                engineAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        ConfigureAudioSource();
        SetEngineActive(false);
    }

    private void OnEnable()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
        }

        if (enemy != null)
        {
            enemy.Initialized += OnEnemyInitialized;
            enemy.Despawned += OnEnemyDespawned;
            enemy.Died += OnEnemyDied;
            enemy.MovementCommanded += OnEnemyMovementCommanded;
        }
    }

    private void OnDisable()
    {
        if (enemy != null)
        {
            enemy.Initialized -= OnEnemyInitialized;
            enemy.Despawned -= OnEnemyDespawned;
            enemy.Died -= OnEnemyDied;
            enemy.MovementCommanded -= OnEnemyMovementCommanded;
        }

        movementCommandedThisFrame = false;
        StopEngineAudio();
        SetEngineActive(false);
    }

    private void LateUpdate()
    {
        if (forceDisabled)
        {
            movementCommandedThisFrame = false;
            return;
        }

        bool isMoving = movementCommandedThisFrame;

        SetEngineActive(isMoving);

        if (isMoving)
        {
            PlayEngineAudio();
        }
        else
        {
            StopEngineAudio();
        }

        movementCommandedThisFrame = false;
    }

    private void OnEnemyInitialized(Enemy _)
    {
        forceDisabled = false;
        movementCommandedThisFrame = false;
        SetEngineActive(false);
        StopEngineAudio();
    }

    private void OnEnemyDespawned(Enemy _)
    {
        forceDisabled = false;
        movementCommandedThisFrame = false;
        SetEngineActive(false);
        StopEngineAudio();
    }

    private void OnEnemyDied(Enemy _)
    {
        forceDisabled = true;
        movementCommandedThisFrame = false;
        SetEngineActive(false);
        StopEngineAudio();
    }

    private void OnEnemyMovementCommanded(Enemy _)
    {
        if (!forceDisabled)
        {
            movementCommandedThisFrame = true;
        }
    }

    private void ConfigureAudioSource()
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

    private void SetEngineActive(bool active)
    {
        if (engineVisual != null && engineVisual.activeSelf != active)
        {
            engineVisual.SetActive(active);
        }
    }

    private void PlayEngineAudio()
    {
        if (engineAudioSource == null || engineSfx == null || engineSfx.Clip == null)
        {
            return;
        }

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

    private void StopEngineAudio()
    {
        if (engineAudioSource != null && engineAudioSource.isPlaying)
        {
            engineAudioSource.Stop();
        }
    }
}
