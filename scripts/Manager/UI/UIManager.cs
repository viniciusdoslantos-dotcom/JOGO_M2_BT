using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Villager UI")]
    public GameObject villagerPanel;
    public TMP_Text villagerNameText;
    public Image jobImage; // NEW: Display current job image
    public Button makeFarmerButton;
    public Button makeLumberjackButton;
    public Button makeKnightButton;

    [Header("Job Images")]
    public Sprite farmerSprite;
    public Sprite lumberjackSprite;
    public Sprite knightSprite;
    public Sprite defaultSprite; // For unemployed villagers

    [Header("Shop UI")]
    public GameObject shopPanel;
    public Button buildHouseButton;
    public Button buildFarmButton;
    public Button buildVillagerButton;  // NEW: Button to place villagers
    public TMP_Text buildHouseCostText;
    public TMP_Text buildFarmCostText;
    public TMP_Text buildVillagerCostText;  // NEW: Display villager cost

    VillagerController currentVillager;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        villagerPanel.SetActive(false);
        shopPanel.SetActive(false);

        // Job assignment buttons
        makeFarmerButton.onClick.AddListener(OnMakeFarmer);
        makeLumberjackButton.onClick.AddListener(OnMakeLumberjack);
        makeKnightButton.onClick.AddListener(OnMakeKnight);

        // Building placement buttons
        buildHouseButton.onClick.AddListener(() => PlacementManager.Instance.StartPlacing(PlacementManager.BuildingType.House));
        buildFarmButton.onClick.AddListener(() => PlacementManager.Instance.StartPlacing(PlacementManager.BuildingType.Farm));
        buildVillagerButton.onClick.AddListener(() => PlacementManager.Instance.StartPlacing(PlacementManager.BuildingType.Villager));  // NEW

        // Display costs
        buildHouseCostText.text = $"Cost: {PlacementManager.houseWoodCost} wood";
        buildFarmCostText.text = $"Cost: {PlacementManager.farmWoodCost} wood";
        buildVillagerCostText.text = $"Cost: {PlacementManager.villagerWoodCost} wood, {PlacementManager.villagerFoodCost} food";  // NEW
    }

    public void OpenVillagerPanel(VillagerController v)
    {
        currentVillager = v;
        villagerPanel.SetActive(true);
        RefreshVillagerPanel();
    }

    public void RefreshVillagerPanel()
    {
        if (currentVillager == null) return;

        // Display villager info with current job
        string jobName = currentVillager.GetJobName();
        villagerNameText.text = $"Villager {currentVillager.id} - {jobName}";

        // Update job image based on current job
        UpdateJobImage();

        // Update button states based on current job
        UpdateJobButtons();
    }

    void UpdateJobImage()
    {
        if (currentVillager == null || jobImage == null) return;

        VillagerJob currentJob = currentVillager.GetCurrentJob();

        // Set sprite based on job type
        if (currentJob is FarmerJob)
            jobImage.sprite = farmerSprite;
        else if (currentJob is LumberjackJob)
            jobImage.sprite = lumberjackSprite;
        else if (currentJob is KnightJob)
            jobImage.sprite = knightSprite;
        else
            jobImage.sprite = defaultSprite; // Unemployed

        // Show/hide image based on whether we have a sprite
        jobImage.gameObject.SetActive(jobImage.sprite != null);
    }

    void UpdateJobButtons()
    {
        if (currentVillager == null) return;

        VillagerJob currentJob = currentVillager.GetCurrentJob();

        // Disable button if villager already has that job
        if (makeFarmerButton)
            makeFarmerButton.interactable = !(currentJob is FarmerJob);

        if (makeLumberjackButton)
            makeLumberjackButton.interactable = !(currentJob is LumberjackJob);

        if (makeKnightButton)
            makeKnightButton.interactable = !(currentJob is KnightJob);
    }

    void OnMakeFarmer()
    {
        if (currentVillager == null) return;

        // Cost 50 food to assign job
        bool ok = GameManager.Instance.SpendFood(50);
        if (!ok)
        {
            Debug.Log("Not enough food to assign Farmer job!");
            return;
        }

        currentVillager.AssignJob<FarmerJob>();
        RefreshVillagerPanel();

        Debug.Log($"✅ {currentVillager.name} is now a Farmer!");
    }

    void OnMakeLumberjack()
    {
        if (currentVillager == null) return;

        // Cost 50 food to assign job
        bool ok = GameManager.Instance.SpendFood(50);
        if (!ok)
        {
            Debug.Log("Not enough food to assign Lumberjack job!");
            return;
        }

        currentVillager.AssignJob<LumberjackJob>();
        RefreshVillagerPanel();

        Debug.Log($"✅ {currentVillager.name} is now a Lumberjack!");
    }

    void OnMakeKnight()
    {
        if (currentVillager == null) return;

        // Cost 100 food to assign Knight job (more expensive!)
        bool ok = GameManager.Instance.SpendFood(100);
        if (!ok)
        {
            Debug.Log("Not enough food to assign Knight job!");
            return;
        }

        currentVillager.AssignJob<KnightJob>();
        RefreshVillagerPanel();

        Debug.Log($"✅ {currentVillager.name} is now a Knight!");
    }

    public void CloseVillagerPanel()
    {
        villagerPanel.SetActive(false);
        currentVillager = null;
    }

    public void OpenShop() => shopPanel.SetActive(true);
    public void CloseShop() => shopPanel.SetActive(false);
}