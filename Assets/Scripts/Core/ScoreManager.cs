using System;
using UnityEngine;

/// <summary>
/// Tracks player score across the current run.
/// </summary>
public class ScoreManager : SingletonBase<ScoreManager>
{
    private const string HighScoreKey = "score.high";

    private int score;
    private int highScore;

    /// <summary>
    /// Current score value.
    /// </summary>
    public int Score => score;

    /// <summary>
    /// Highest score persisted between runs.
    /// </summary>
    public int HighScore => highScore;

    /// <summary>
    /// Fired when score changes.
    /// </summary>
    public event Action<int> ScoreChanged;

    /// <summary>
    /// Fired when the high score changes.
    /// </summary>
    public event Action<int> HighScoreChanged;

    protected override void Awake()
    {
        base.Awake();

        if (!ReferenceEquals(Instance, this))
        {
            return;
        }

        highScore = Mathf.Max(0, PlayerDataStore.GetInt(HighScoreKey, 0));
    }

    /// <summary>
    /// Add points to the score.
    /// </summary>
    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        score += amount;
        ScoreChanged?.Invoke(score);

        if (score > highScore)
        {
            highScore = score;
            PlayerDataStore.SetInt(HighScoreKey, highScore);
            HighScoreChanged?.Invoke(highScore);
        }
    }

    /// <summary>
    /// Reset score to zero.
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        ScoreChanged?.Invoke(score);
    }
}
