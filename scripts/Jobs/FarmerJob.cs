using UnityEngine;

public class FarmerJob : VillagerJob
{
    [Header("Farmer Settings")]
    public float workDistance = 1.2f;

    protected override void SetupBehaviorTree()
    {
        var farmerAction = new ActionNode(() => FarmingBehavior());
        rootNode = farmerAction;
    }

    protected override void CleanupJob()
    {
        // Release farm assignment
        if (bb.farmAssigned != null)
        {
            bb.farmAssigned.Unassign();
            bb.farmAssigned = null;
        }
    }

    private NodeState FarmingBehavior()
    {
        // Find a farm if not assigned
        if (bb.farmAssigned == null)
        {
            var farm = GameManager.Instance.GetAvailableFarm();
            if (farm != null)
            {
                bb.farmAssigned = farm;
                farm.AssignFarmer(villager);
            }
            else
            {
                // No farm available, wander
                villager.IdleBehavior();
                return NodeState.RUNNING;
            }
        }

        // Move to farm work position
        float dist = Vector3.Distance(villager.transform.position, bb.farmAssigned.workPosition.position);
        if (dist > workDistance)
        {
            villager.agent.isStopped = false;
            villager.agent.SetDestination(bb.farmAssigned.workPosition.position);
            villager.SetAnimatorMoving(true);
            return NodeState.RUNNING;
        }
        else
        {
            // Arrived at farm - work here
            villager.agent.isStopped = true;
            villager.SetAnimatorMoving(false);
            return NodeState.SUCCESS;
        }
    }
}