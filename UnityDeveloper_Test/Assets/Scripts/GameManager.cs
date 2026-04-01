using System;
using UnityEngine;

/// <summary>
/// Created this singleton class to get access to the persistant data, like score and timer
/// </summary>

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int score = 0;
    private int maxCubes = 5;
    private bool isGameOver = false;

    // events for score, time change and on game over
    public static event Action<int> OnScoreChanged;
    public static event Action<float> OnTimeChanged;
    public static event Action<bool> OnGameOver;

    [Header("Timer Settings")]
    [SerializeField] private float timeLimit = 120f; // 2 minutes in seconds
    private float timeRemaining;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        timeRemaining = timeLimit;
    }

    private void Update()
    {
        if (isGameOver) return;

        // Handle Timer Count Down
        timeRemaining -= Time.deltaTime;

        // notify the UI to update the text
        OnTimeChanged?.Invoke(timeRemaining);

        // check if time ran out
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            TriggerGameOver(false); // false means the player lost
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;

        score += amount;
        OnScoreChanged?.Invoke(score);

        // check if player collected all cubes
        if (score >= maxCubes)
        {
            TriggerGameOver(true); // true means the player won
        }
    }

    // call this from anywhere to end the game
    public void TriggerGameOver(bool isWin)
    {
        if (isGameOver) return;
        isGameOver = true;

        OnGameOver?.Invoke(isWin);

        Time.timeScale = 0f; // Pause the game
    }
}
