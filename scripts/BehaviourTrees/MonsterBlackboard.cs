using UnityEngine;
using UnityEngine.AI;

public class MonsterBlackboard
{
    public NavMeshAgent agent;
    public Transform target; // Town Hall
    public Vector3 spawnPoint;
    public int spawnPointIndex;
    public bool isReturningHome;
    public float lastAttackTime;
}