using TMPro;
using UnityEngine;

/// <summary>
/// Shows the score text and update the timer by subscribing the GameManger events
/// </summary>

public class InGameUI : MonoBehaviour
{
    [Header("Score TextBox")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Timer TextBox")]
    [SerializeField] private TextMeshProUGUI timerText;

    private void OnEnable()
    {
        GameManager.OnScoreChanged += UpdateScoreOnUI;
        GameManager.OnTimeChanged += UpdateTimerOnUI;
    }

    private void UpdateScoreOnUI(int score)
    {
        scoreText.text = $"Collectible: {score}/5";
    }

    private void UpdateTimerOnUI(float timeRemaining)
    {
        // format the float into Minutes and Seconds
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged -= UpdateScoreOnUI;
        GameManager.OnTimeChanged -= UpdateTimerOnUI;
    }
}
