using TMPro;
using UnityEngine;

/// <summary>
/// Shows game over panel by subscribing to the GameManger OnGameOver event.
/// </summary>

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI titleText;

    private void Awake()
    {
        // disable just the panel visuals, keeping this script active and listening!
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameOver += HandleGameOver;
    }

    private void HandleGameOver(bool isWin)
    {
        // change the text based on the outcome
        if (isWin)
        {
            titleText.text = "You Won!";
            titleText.color = Color.green;
        }
        else
        {
            titleText.text = "Game Over!";
            titleText.color = Color.red;
        }

        // show the panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= HandleGameOver;
    }
}
