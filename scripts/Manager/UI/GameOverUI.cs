using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";

    void Start()
    {
        // Hide panel initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Pause the game
        Time.timeScale = 0f;

        // Optional: Play sound effect or animation here
    }

    void RestartGame()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ReturnToMainMenu()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;

        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnDestroy()
    {
        // Cleanup listeners
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
        }
    }
}