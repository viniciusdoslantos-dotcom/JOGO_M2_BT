using UnityEngine;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    public static MonsterSpawner Instance;

    [Header("Spawning")]
    public GameObject monsterPrefab;
    public Transform[] spawnPoints;

    [Header("Spawn Settings")]
    public int baseMonsterCount = 3; // Monsters per night on Day 1
    public int monstersPerDayIncrease = 2; // +2 monsters each night
    public float spawnInterval = 1f; // Time between each spawn

    private List<Monster> activeMonsters = new List<Monster>();
    private int monstersToSpawnThisNight = 0;
    private int monstersSpawnedSoFar = 0;
    private float spawnTimer = 0f;
    private bool hasSpawnedThisNight = false;

    void Awake() => Instance = this;

    void Update()
    {
        if (GameManager.Instance == null) return;

        // When night falls, start spawning
        if (GameManager.Instance.isNight)
        {
            if (!hasSpawnedThisNight)
            {
                StartNightSpawning();
            }

            // Spawn monsters gradually
            if (monstersSpawnedSoFar < monstersToSpawnThisNight)
            {
                spawnTimer += Time.deltaTime;
                if (spawnTimer >= spawnInterval)
                {
                    SpawnMonster();
                    spawnTimer = 0f;
                }
            }
        }
        else
        {
            // Day time - send all monsters home
            if (hasSpawnedThisNight)
            {
                SendMonstersHome();
                hasSpawnedThisNight = false;
                monstersSpawnedSoFar = 0;
            }
        }

        // Clean up dead/null monsters
        activeMonsters.RemoveAll(m => m == null);
    }

    void StartNightSpawning()
    {
        hasSpawnedThisNight = true;

        // Calculate how many monsters for this night
        monstersToSpawnThisNight = baseMonsterCount + (monstersPerDayIncrease * (GameManager.Instance.currentDay - 1));

        Debug.Log($"👹 Night {GameManager.Instance.currentDay}: Spawning {monstersToSpawnThisNight} monsters!");
    }

    void SpawnMonster()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points assigned!");
            return;
        }

        int spawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[spawnIndex];

        GameObject monsterObj = Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        Monster monster = monsterObj.GetComponent<Monster>();

        if (monster != null)
        {
            monster.spawnPoint = spawnPoint.position;
            monster.spawnPointIndex = spawnIndex;
            activeMonsters.Add(monster);
        }

        monstersSpawnedSoFar++;
    }

    void SendMonstersHome()
    {
        Debug.Log("🌅 Dawn breaks - monsters retreating!");

        foreach (Monster monster in activeMonsters)
        {
            if (monster != null)
            {
                monster.isReturningHome = true;
            }
        }
    }

    public void UnregisterMonster(Monster monster)
    {
        activeMonsters.Remove(monster);
    }
}