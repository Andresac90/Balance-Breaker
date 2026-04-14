using UnityEngine;
using TMPro;

/// <summary>
/// Attach to a Canvas. Assign TMP text fields in the Inspector.
/// Wire the GameManager events to the public methods below.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ballsFallenText;
    public TextMeshProUGUI statusText;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI winScoreText;

    public void OnWaveStart(int wave, int ballCount)
    {
        if (waveText != null)
            waveText.text = $"Wave {wave} - {ballCount} ball{(ballCount > 1 ? "s" : "")}";
        if (statusText != null)
            statusText.text = "";
    }

    public void OnTimerUpdate(float timeRemaining)
    {
        if (timerText != null)
            timerText.text = $"Time: {Mathf.Max(0, timeRemaining):F1}s";
    }

    public void OnScoreUpdate(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    public void OnBallFell(int totalFallen)
    {
        if (ballsFallenText != null)
            ballsFallenText.text = $"Balls Lost: {totalFallen}";
    }

    public void OnGameOver(int wavesCompleted, float totalTime)
    {
        if (statusText != null)
            statusText.text = "GAME OVER";
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverScoreText != null)
                gameOverScoreText.text = $"Wave {wavesCompleted}\nTime: {totalTime:F1}s";
        }
    }

    public void OnGameWon()
    {
        if (statusText != null)
            statusText.text = "YOU WIN!";
        if (winPanel != null)
            winPanel.SetActive(true);
    }
}
