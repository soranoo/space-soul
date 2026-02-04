using System;
using UnityEngine;

/// <summary>
/// Tracks player score across the current run.
/// </summary>
public class ScoreManager : SingletonBase<ScoreManager>
{
    private int score;

    /// <summary>
    /// Current score value.
    /// </summary>
    public int Score => score;

    /// <summary>
    /// Fired when score changes.
    /// </summary>
    public event Action<int> ScoreChanged;

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
