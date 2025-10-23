using UnityEngine;

public class Farm : MonoBehaviour
{
    public Transform workPosition;
    public bool HasFarmer => assignedFarmer != null;
    VillagerController assignedFarmer;

    [Header("Production")]
    public int foodPerDay = 200; // free farm gives 200/day
    public float productionInterval = 30f; // seconds (for demo)
    float timer;

    void Start()
    {
        timer = 0;
        GameManager.Instance.farms.Add(this);
    }

    void Update()
    {
        if (HasFarmer)
        {
            timer += Time.deltaTime;
            if (timer >= productionInterval)
            {
                GameManager.Instance.AddFood(foodPerDay);
                timer = 0;
            }
        }
    }

    public void AssignFarmer(VillagerController v)
    {
        assignedFarmer = v;
        // The FarmerJob component will handle the blackboard assignment
    }

    public void Unassign()
    {
        assignedFarmer = null;
    }
}