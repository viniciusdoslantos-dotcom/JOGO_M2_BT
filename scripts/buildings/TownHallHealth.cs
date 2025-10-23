using UnityEngine;
using TMPro;

public class TownHallHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 500f;
    public float currentHealth;

    [Header("UI")]
    public TMP_Text healthText;
    public GameOverUI gameOverUI;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();

        // Find GameOverUI if not assigned
        if (gameOverUI == null)
        {
            gameOverUI = FindObjectOfType<GameOverUI>();
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void UpdateUI()
    {
        if (healthText != null)
        {
            healthText.text = $"Town Hall: {currentHealth:F0}/{maxHealth:F0}";
        }
    }

    void GameOver()
    {
        Debug.Log("💀 GAME OVER - Town Hall Destroyed!");

        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogError("GameOverUI reference is missing!");
        }
    }
}