using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current score.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text scoreText;

    private ScoreManager scoreManager;

    private void OnEnable()
    {
        scoreManager = ScoreManager.Instance;
        Subscribe();
        RefreshImmediate();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreChanged += OnScoreChanged;
        }
    }

    private void Unsubscribe()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -= OnScoreChanged;
        }
    }

    private void RefreshImmediate()
    {
        if (scoreManager == null)
        {
            return;
        }

        OnScoreChanged(scoreManager.Score);
    }

    private void OnScoreChanged(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = Mathf.Max(0, score).ToString();
        }
    }
}
