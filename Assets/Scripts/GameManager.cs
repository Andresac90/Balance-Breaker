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

        // Random position on table surface
        float randX = Random.Range(-0.7f, 0.7f);
        float randZ = Random.Range(-0.5f, 0.5f);
        Vector3 spawnPos = table.TransformPoint(new Vector3(randX, 0.25f, randZ));

        GameObject ball = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        ball.SetActive(true);

        // Wire up the agent's table reference
        BallAgent agent = ball.GetComponent<BallAgent>();
        if (agent != null)
        {
            agent.table = table;
        }

        // Listen for when this ball falls
        BallFallDetector detector = ball.GetComponent<BallFallDetector>();
        if (detector == null)
            detector = ball.AddComponent<BallFallDetector>();
        detector.onFell = () => OnBallFellOff(ball);

        activeBalls.Add(ball);
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
        }
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
