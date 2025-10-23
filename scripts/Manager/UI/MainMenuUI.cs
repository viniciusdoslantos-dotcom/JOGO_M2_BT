using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI References")]
    public Button playButton;
    public Button quitButton;
    public TMP_Text titleText;

    [Header("Settings")]
    public string gameSceneName = "GameScene";

    void Start()
    {
        // Make sure time is running
        Time.timeScale = 1f;

        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(PlayGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void PlayGame()
    {
        // Load the game scene
        SceneManager.LoadScene(gameSceneName);
    }

    void QuitGame()
    {
        // Log for testing in editor
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        // Stop playing in editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Quit the application in build
            Application.Quit();
#endif
    }

    void OnDestroy()
    {
        // Cleanup listeners
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(PlayGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(QuitGame);
        }
    }
}