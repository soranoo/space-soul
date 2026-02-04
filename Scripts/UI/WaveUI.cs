using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays wave number, enemies left, and a wave start splash.
/// </summary>
public class WaveUI : MonoBehaviour
{
    [Header("Wave UI")]
    [SerializeField] private TMP_Text waveNumberText;
    [SerializeField] private TMP_Text enemiesLeftText;

    [Header("Wave Splash")]
    [SerializeField] private GameObject splashRoot;
    [SerializeField] private TMP_Text splashText;
    [SerializeField] private CanvasGroup splashCanvasGroup;
    [SerializeField] private RectTransform splashRect;
    [SerializeField] private float splashHoldSeconds = 1f;
    [SerializeField] private float splashFadeSeconds = 0.35f;
    [SerializeField] private float splashScaleFrom = 0.9f;
    [SerializeField] private float splashScaleTo = 1f;

    private WaveManager waveManager;
    private Sequence splashSequence;

    private void Awake()
    {
        if (splashRoot != null)
        {
            if (splashCanvasGroup == null)
            {
                splashCanvasGroup = splashRoot.GetComponent<CanvasGroup>();
                if (splashCanvasGroup == null)
                {
                    splashCanvasGroup = splashRoot.AddComponent<CanvasGroup>();
                }
            }

            if (splashRect == null)
            {
                splashRect = splashRoot.GetComponent<RectTransform>();
            }
        }
    }

    private void OnEnable()
    {
        waveManager = WaveManager.Instance;
        Subscribe();
        RefreshImmediate();
    }

    private void OnDisable()
    {
        Unsubscribe();
        KillSplashTween();
    }

    private void Subscribe()
    {
        if (waveManager == null)
        {
            return;
        }

        waveManager.WaveStarted += OnWaveStarted;
        waveManager.WaveCompleted += OnWaveCompleted;
        waveManager.EnemySpawned += OnEnemyCountChanged;
        waveManager.EnemyDied += OnEnemyCountChanged;
    }

    private void Unsubscribe()
    {
        if (waveManager == null)
        {
            return;
        }

        waveManager.WaveStarted -= OnWaveStarted;
        waveManager.WaveCompleted -= OnWaveCompleted;
        waveManager.EnemySpawned -= OnEnemyCountChanged;
        waveManager.EnemyDied -= OnEnemyCountChanged;
    }

    private void RefreshImmediate()
    {
        if (waveManager == null)
        {
            return;
        }

        UpdateWaveNumberText(waveManager.CurrentWaveNumber);
        UpdateEnemiesLeftText(waveManager.EnemiesAlive);
    }

    private void OnWaveStarted(int waveNumber)
    {
        UpdateWaveNumberText(waveNumber);
        UpdateEnemiesLeftText(waveManager != null ? waveManager.EnemiesAlive : 0);
        PlaySplash(waveNumber);
    }

    private void OnWaveCompleted(int waveNumber)
    {
        UpdateEnemiesLeftText(0);
    }

    private void OnEnemyCountChanged(Enemy _)
    {
        if (waveManager == null)
        {
            return;
        }

        UpdateEnemiesLeftText(waveManager.EnemiesAlive);
    }

    private void UpdateWaveNumberText(int waveNumber)
    {
        if (waveNumberText != null)
        {
            waveNumberText.text = $"Wave {Mathf.Max(0, waveNumber)}";
        }
    }

    private void UpdateEnemiesLeftText(int enemiesLeft)
    {
        if (enemiesLeftText != null)
        {
            enemiesLeftText.text = $"Enemies: {Mathf.Max(0, enemiesLeft)}";
        }
    }

    private void PlaySplash(int waveNumber)
    {
        if (splashRoot == null || splashText == null || splashCanvasGroup == null)
        {
            return;
        }

        splashText.text = $"Wave {Mathf.Max(0, waveNumber)}";
        splashRoot.SetActive(true);
        splashCanvasGroup.alpha = 0f;

        if (splashRect != null)
        {
            splashRect.localScale = Vector3.one * splashScaleFrom;
        }

        KillSplashTween();

        splashSequence = DOTween.Sequence();
        splashSequence.Append(splashCanvasGroup.DOFade(1f, splashFadeSeconds));

        if (splashRect != null)
        {
            splashSequence.Join(splashRect.DOScale(splashScaleTo, splashFadeSeconds).SetEase(Ease.OutBack));
        }

        splashSequence.AppendInterval(splashHoldSeconds);
        splashSequence.Append(splashCanvasGroup.DOFade(0f, splashFadeSeconds));
        splashSequence.OnComplete(() =>
        {
            if (splashRoot != null)
            {
                splashRoot.SetActive(false);
            }
        });
    }

    private void KillSplashTween()
    {
        if (splashSequence != null)
        {
            splashSequence.Kill();
            splashSequence = null;
        }
    }
}
