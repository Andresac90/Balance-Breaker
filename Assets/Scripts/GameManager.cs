using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    public Transform table;
    public GameObject ballPrefab;

    [Header("Wave Settings")]
    public int totalWaves = 10;
    public float waveDuration = 20f;
    public int startingBalls = 1;
    public int maxBalls = 5;
    public int maxFallsAllowed = 3;

    [Header("Events")]
    public UnityEvent<int, int> OnWaveStart;       // (waveNumber, ballCount)
    public UnityEvent<int> OnBallFell;             // (totalFallen)
    public UnityEvent<int, float> OnGameOver;      // (wavesCompleted, totalTime)
    public UnityEvent OnGameWon;
    public UnityEvent<float> OnTimerUpdate;        // (timeRemaining)
    public UnityEvent<int> OnScoreUpdate;          // (score)

    // State
    private int currentWave = 0;
    private int ballsFallen = 0;
    private int score = 0;
    private float totalTimeSurvived = 0f;
    private float waveTimer = 0f;
    private bool gameActive = false;
    private List<GameObject> activeBalls = new List<GameObject>();

    public void StartGame()
    {
        currentWave = 0;
        ballsFallen = 0;
        score = 0;
        totalTimeSurvived = 0f;
        gameActive = true;

        StartNextWave();
    }

    private void Update()
    {
        if (!gameActive) return;

        totalTimeSurvived += Time.deltaTime;
        waveTimer -= Time.deltaTime;
        OnTimerUpdate?.Invoke(waveTimer);

        if (waveTimer <= 0f)
        {
            // Wave survived! Award bonus points
            int ballsRemaining = activeBalls.Count;
            score += ballsRemaining * 100 + (int)(waveDuration * 10);
            OnScoreUpdate?.Invoke(score);

            ClearBalls();
            StartNextWave();
        }
    }

    private void StartNextWave()
    {
        currentWave++;

        if (currentWave > totalWaves)
        {
            gameActive = false;
            OnGameWon?.Invoke();
            return;
        }

        // Calculate balls for this wave (scales up each wave)
        int ballCount = Mathf.Min(startingBalls + (currentWave - 1), maxBalls);
        waveTimer = waveDuration;

        OnWaveStart?.Invoke(currentWave, ballCount);

        StartCoroutine(SpawnBallsDelayed(ballCount));
    }

    private IEnumerator SpawnBallsDelayed(int count)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnBall();
            yield return new WaitForSeconds(0.5f); // Stagger spawns
        }
    }

    private void SpawnBall()
    {
        if (ballPrefab == null || table == null) return;

        // Get table top in world space directly from renderer
        Renderer tableRenderer = table.GetComponent<Renderer>();

        // Spawn at table center first, then offset in world space X/Z only
        Vector3 tableCenter = tableRenderer != null
            ? tableRenderer.bounds.center
            : table.position;

        float tableTopY = tableRenderer != null
            ? tableRenderer.bounds.center.y + tableRenderer.bounds.extents.y + 0.15f
            : table.position.y + 0.25f;

        // Use small random offset in WORLD space X/Z from table center
        float randX = Random.Range(-0.2f, 0.2f);
        float randZ = Random.Range(-0.15f, 0.15f);

        Vector3 spawnPos = new Vector3(tableCenter.x + randX, tableTopY, tableCenter.z + randZ);

        GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        ball.SetActive(true);

        BallAgent agent = ball.GetComponent<BallAgent>();
        if (agent != null)
        {
            agent.table = table;
            agent.isGameplay = true; 
        }

        // Small delay before wiring detector so physics can settle
        StartCoroutine(WireDetectorDelayed(ball));

        activeBalls.Add(ball);
    }

    private IEnumerator WireDetectorDelayed(GameObject ball)
    {
        yield return new WaitForSeconds(0.3f);

        if (ball == null) yield break; 

        BallFallDetector detector = ball.GetComponent<BallFallDetector>();
        if (detector == null) detector = ball.AddComponent<BallFallDetector>();

        GameObject capturedBall = ball;
        detector.onFell = () => OnBallFellOff(capturedBall);
    }

    private void OnBallFellOff(GameObject ball)
    {
        activeBalls.Remove(ball);
        Destroy(ball);

        ballsFallen++;
        OnBallFell?.Invoke(ballsFallen);

        if (ballsFallen >= maxFallsAllowed)
        {
            gameActive = false;
            ClearBalls();
            OnGameOver?.Invoke(currentWave, totalTimeSurvived);
            return;
        }

        // Respawn a replacement ball after a short delay
        if (gameActive)
        {
            StartCoroutine(RespawnBallDelayed());
        }
    }

    private IEnumerator RespawnBallDelayed()
    {
        yield return new WaitForSeconds(1f);
        if (gameActive)
            SpawnBall();
    }

    private void ClearBalls()
    {
        foreach (var ball in activeBalls)
        {
            if (ball != null) Destroy(ball);
        }
        activeBalls.Clear();
    }
}
