using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to any GameObject. Calls GameManager.StartGame() on Space press or button click.
/// </summary>
public class GameStarter : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject startPanel; // Optional: a "Press Space to Start" panel

    private bool started = false;

    private void Update()
    {
        if (!started && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (started) return;
        started = true;

        if (startPanel != null)
            startPanel.SetActive(false);

        if (gameManager != null)
            gameManager.StartGame();
    }

    /// <summary>Call from Restart button</summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
