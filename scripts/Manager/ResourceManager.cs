using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [Header("Resources")]
    public int food = 200;
    public int wood = 0;

    [Header("UI References")]
    public TextMeshProUGUI foodText;
    public TextMeshProUGUI woodText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public bool SpendFood(int amount)
    {
        if (food >= amount)
        {
            food -= amount;
            UpdateUI();
            return true;
        }
        else
        {
            Debug.Log("Not enough food!");
            return false;
        }
    }

    public void AddWood(int amount)
    {
        wood += amount;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (foodText != null)
            foodText.text = "Food: " + food;

        if (woodText != null)
            woodText.text = "Wood: " + wood;
    }
}
