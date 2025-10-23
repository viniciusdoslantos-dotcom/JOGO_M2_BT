using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class House : MonoBehaviour
{
    [Header("House Settings")]
    public int maxCapacity = 4; // Maximum villagers per house
    public Transform doorPosition; // Where villagers gather before entering
    public Transform insidePosition; // Where villagers stand inside (optional, for visualization)

    [Header("Interior Settings")]
    public bool showVillagersInside = true; // Show villagers inside or hide them completely
    public float interiorSpacing = 1f; // Space between villagers inside

    [Header("Exit Settings")]
    public float exitInterval = 0.5f; // Time between each villager exiting
    public float exitDistance = 2f; // How far from door they walk after exiting

    private List<VillagerController> residents = new List<VillagerController>();
    private bool isReleasingVillagers = false;

    public bool HasSpace => residents.Count < maxCapacity;
    public int CurrentOccupants => residents.Count;

    void Start()
    {
        // Auto-register with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterHouse(this);
            Debug.Log($"🏠 {name} registered with capacity: {maxCapacity}");
        }

        // Create default positions if not set
        if (doorPosition == null)
        {
            GameObject door = new GameObject("DoorPosition");
            door.transform.SetParent(transform);
            door.transform.localPosition = new Vector3(0, 0, 2f);
            doorPosition = door.transform;
        }

        if (insidePosition == null)
        {
            GameObject inside = new GameObject("InsidePosition");
            inside.transform.SetParent(transform);
            inside.transform.localPosition = Vector3.zero;
            insidePosition = inside.transform;
        }
    }

    /// <summary>
    /// Assigns a villager to this house if there's space
    /// </summary>
    public bool AssignVillager(VillagerController villager)
    {
        if (!HasSpace)
        {
            Debug.LogWarning($"{name} is full! ({residents.Count}/{maxCapacity})");
            return false;
        }

        if (residents.Contains(villager))
        {
            Debug.LogWarning($"{villager.name} is already assigned to {name}");
            return true;
        }

        residents.Add(villager);
        Debug.Log($"✅ {villager.name} assigned to {name} ({residents.Count}/{maxCapacity})");
        return true;
    }

    /// <summary>
    /// Called when a villager reaches the door and enters
    /// </summary>
    public void OnVillagerEnter(VillagerController villager)
    {
        if (!residents.Contains(villager))
        {
            Debug.LogWarning($"{villager.name} tried to enter {name} but isn't assigned!");
            return;
        }

        Debug.Log($"🚪 {villager.name} entered {name}");

        if (showVillagersInside)
        {
            // Position villager inside the house
            PositionVillagerInside(villager);
        }
        else
        {
            // Hide villager completely
            villager.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Positions a villager inside the house in a grid pattern
    /// </summary>
    void PositionVillagerInside(VillagerController villager)
    {
        int index = residents.IndexOf(villager);

        // Calculate grid position (2x2 grid for 4 villagers)
        int cols = 2;
        int row = index / cols;
        int col = index % cols;

        Vector3 offset = new Vector3(
            (col - 0.5f) * interiorSpacing,
            0,
            row * interiorSpacing
        );

        Vector3 finalPos = insidePosition.position + offset;

        // Move villager to interior position
        villager.transform.position = finalPos;
        villager.transform.rotation = Quaternion.Euler(0, 180, 0); // Face forward

        // Make sure they're visible
        villager.gameObject.SetActive(true);
    }

    /// <summary>
    /// Releases all villagers one by one in the morning
    /// </summary>
    public IEnumerator ReleaseVillagersOneByOne()
    {
        if (isReleasingVillagers)
        {
            Debug.LogWarning($"{name} is already releasing villagers!");
            yield break;
        }

        if (residents.Count == 0)
        {
            Debug.Log($"{name} has no residents to release.");
            yield break;
        }

        isReleasingVillagers = true;
        Debug.Log($"🌅 {name} releasing {residents.Count} villagers...");

        // Create a copy to avoid modification issues
        List<VillagerController> toRelease = new List<VillagerController>(residents);

        foreach (var villager in toRelease)
        {
            if (villager != null)
            {
                ReleaseVillager(villager);
                yield return new WaitForSeconds(exitInterval);
            }
        }

        isReleasingVillagers = false;
        Debug.Log($"✅ {name} finished releasing villagers");
    }

    /// <summary>
    /// Releases a single villager from the house
    /// </summary>
    void ReleaseVillager(VillagerController villager)
    {
        Debug.Log($"👋 {villager.name} leaving {name}");

        // Make sure villager is active
        villager.gameObject.SetActive(true);

        // Position at door
        villager.transform.position = doorPosition.position;

        // Call the villager's leave home method
        villager.LeaveHome();

        // Make them walk away from the door slightly
        Vector3 exitPoint = doorPosition.position + doorPosition.forward * exitDistance;
        if (villager.agent != null)
        {
            villager.agent.isStopped = false;
            villager.agent.SetDestination(exitPoint);
        }
    }

    /// <summary>
    /// Removes a villager from this house
    /// </summary>
    public void RemoveVillager(VillagerController villager)
    {
        if (residents.Contains(villager))
        {
            residents.Remove(villager);
            Debug.Log($"❌ {villager.name} removed from {name} ({residents.Count}/{maxCapacity})");
        }
    }

    /// <summary>
    /// Visual debug info
    /// </summary>
    void OnDrawGizmos()
    {
        if (doorPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(doorPosition.position, 0.5f);
            Gizmos.DrawLine(doorPosition.position, doorPosition.position + doorPosition.forward * 2f);
        }

        if (insidePosition != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(insidePosition.position, Vector3.one * 0.5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Show capacity info
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 3f,
                $"{name}\n{residents.Count}/{maxCapacity} villagers"
            );
        }
    }
}