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
    [SerializeField, Min(0.01f)] private float maxAudibleDistance = 20f;

    private bool forceDisabled;
    private bool movementCommandedThisFrame;
    private Enemy enemy;
    private float maxEngineVolume = 1f;

#if UNITY_EDITOR
    [Header("Gizmos")]
    [SerializeField] private bool showDistanceGizmo = true;
    [SerializeField] private Color distanceGizmoColor = new Color(0.2f, 0.8f, 1f, 0.8f);
#endif

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

        maxEngineVolume = 1f;

        if (engineSfx != null)
        {
            engineAudioSource.clip = engineSfx.Clip;
            engineSfx.Source?.ApplyTo(engineAudioSource);
            engineAudioSource.loop = true;
            maxEngineVolume = Mathf.Clamp01(engineSfx.Source != null ? engineSfx.Source.Volume : 1f);
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

        UpdateEngineVolumeByDistance();
    }

    private void UpdateEngineVolumeByDistance()
    {
        if (engineAudioSource == null || enemy == null)
        {
            return;
        }

        float distance = enemy.GetDistanceToPlayer();
        float normalized = 1f - Mathf.Clamp01(distance / maxAudibleDistance);
        engineAudioSource.volume = Mathf.Clamp01(maxEngineVolume * normalized);
    }

    private void StopEngineAudio()
    {
        if (engineAudioSource != null && engineAudioSource.isPlaying)
        {
            engineAudioSource.Stop();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDistanceGizmo)
        {
            return;
        }

        Gizmos.color = distanceGizmoColor;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0f, maxAudibleDistance));
    }
#endif
}
