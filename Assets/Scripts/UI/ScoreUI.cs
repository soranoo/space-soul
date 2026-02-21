using TMPro;
using UnityEngine;

/// <summary>
/// Displays the current score.
/// </summary>
public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text highScoreText;

    [Header("Formatting")]
    [SerializeField] private string scoreFormat = "{0}";
    [SerializeField] private string highScoreFormat = "Best {0}";

    private ScoreManager scoreManager;
    private int currentScore;
    private int currentHighScore;

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
            scoreManager.HighScoreChanged += OnHighScoreChanged;
        }
    }

    private void Unsubscribe()
    {
        if (scoreManager != null)
        {
            scoreManager.ScoreChanged -= OnScoreChanged;
            scoreManager.HighScoreChanged -= OnHighScoreChanged;
        }
    }

    private void RefreshImmediate()
    {
        if (scoreManager == null)
        {
            return;
        }

        OnScoreChanged(scoreManager.Score);
        OnHighScoreChanged(scoreManager.HighScore);
    }

    private void OnScoreChanged(int score)
    {
        currentScore = Mathf.Max(0, score);
        UpdateTexts();
    }

    private void OnHighScoreChanged(int highScore)
    {
        currentHighScore = Mathf.Max(0, highScore);
        UpdateTexts();
    }

    private void UpdateTexts()
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, currentScore);
        }

        if (highScoreText != null)
        {
            highScoreText.text = string.Format(highScoreFormat, currentHighScore);
        }
    }
}
