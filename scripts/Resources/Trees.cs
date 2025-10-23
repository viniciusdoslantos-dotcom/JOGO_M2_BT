using UnityEngine;

public class Tree : MonoBehaviour
{
    public int woodAmount = 100;
    public bool assigned = false;
    VillagerController assignedVillager;

    void Start()
    {
        GameManager.Instance.trees.Add(this);
    }

    public void AssignLumberjack(VillagerController v)
    {
        assignedVillager = v;
        assigned = true;
    }

    public int Harvest(int amount)
    {
        int harvested = Mathf.Min(amount, woodAmount);
        woodAmount -= harvested;
        if (woodAmount <= 0) OnDepleted();
        return harvested;
    }

    public bool IsDepleted() => woodAmount <= 0;

    void OnDepleted()
    {
        // Optionally play destroy animation, disable collider etc.
        assigned = false;

        // Clear the tree assignment from the lumberjack's job component
        if (assignedVillager != null)
        {
            LumberjackJob lumberjackJob = assignedVillager.GetComponent<LumberjackJob>();
            if (lumberjackJob != null)
            {
                // The lumberjack will find a new tree on next behavior tick
            }
            assignedVillager = null;
        }

        // Remove from manager list
        if (GameManager.Instance != null && GameManager.Instance.trees != null)
        {
            GameManager.Instance.trees.Remove(this);
        }

        // Safe destruction - handles both runtime and edit mode
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
#else
    Destroy(gameObject);
#endif
    

    // Remove from manager list
    GameManager.Instance.trees.Remove(this);

        // For prototype: destroy tree
        Destroy(gameObject);
    }
}