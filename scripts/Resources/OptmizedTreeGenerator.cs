using UnityEngine;
using System.Collections.Generic;

public class OptimizedTreeGenerator : MonoBehaviour
{
    [Header("Tree Settings")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private int treeCount = 1000;
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.2f;
    [SerializeField] private Vector3 rotationOffset = new Vector3(-90, 0, 0);

    [Header("Spawn Area")]
    [SerializeField] private Vector2 areaSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 areaOffset = Vector2.zero;
    [SerializeField] private LayerMask groundLayer;

    [Header("Distribution")]
    [SerializeField] private float minTreeDistance = 3f;
    [SerializeField] private bool usePoisson = true;
    [SerializeField] private int poissonAttempts = 30;

    [Header("Exclusion Zones")]
    [SerializeField] private bool createCenterClearing = false;
    [SerializeField] private float clearingRadius = 20f;
    [SerializeField] private Vector2 clearingCenter = Vector2.zero;

    [Header("Optimization")]
    [Tooltip("WARNING: Must be FALSE for harvestable trees!")]
    [SerializeField] private bool useCombineMeshes = false;
    [Tooltip("WARNING: Must be FALSE for harvestable trees!")]
    [SerializeField] private bool useStaticBatching = false;
    [SerializeField] private int maxTreesPerChunk = 100;

    [Header("Generation Controls")]
    [SerializeField] private bool generateOnStart = false;

    private List<GameObject> spawnedTrees = new List<GameObject>();
    private Transform treeParent;

    void Start()
    {
        if (generateOnStart)
        {
            GenerateTrees();
        }
    }

    public void GenerateTrees()
    {
        if (useCombineMeshes || useStaticBatching)
        {
            Debug.LogWarning("⚠️ Mesh combining/static batching enabled! Trees won't be individually harvestable. Disable these for dynamic tree destruction.");
        }

        ClearTrees();

        if (treePrefabs == null || treePrefabs.Length == 0)
        {
            Debug.LogError("No tree prefabs assigned!");
            return;
        }

        Debug.Log("Starting tree generation...");
        treeParent = new GameObject("Generated Trees").transform;

        List<Vector3> positions = usePoisson ?
            GeneratePoissonPositions() :
            GenerateRandomPositions();

        Debug.Log($"Generated {positions.Count} positions, spawning trees...");
        SpawnTrees(positions);

        if (useCombineMeshes)
        {
            Debug.Log("Combining meshes...");
            CombineMeshesByMaterial();
        }
        else if (useStaticBatching)
        {
            Debug.Log("Applying static batching...");
            StaticBatchingUtility.Combine(treeParent.gameObject);
        }

        Debug.Log("Tree generation complete!");
    }

    private List<Vector3> GeneratePoissonPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        List<Vector2> grid = new List<Vector2>();

        float cellSize = minTreeDistance / Mathf.Sqrt(2);
        int gridW = Mathf.CeilToInt(areaSize.x / cellSize);
        int gridH = Mathf.CeilToInt(areaSize.y / cellSize);
        Vector2[,] gridArray = new Vector2[gridW, gridH];

        for (int i = 0; i < gridW; i++)
            for (int j = 0; j < gridH; j++)
                gridArray[i, j] = Vector2.one * -1;

        Vector2 firstPoint = new Vector2(
            Random.Range(0, areaSize.x),
            Random.Range(0, areaSize.y)
        );
        grid.Add(firstPoint);

        int gx = Mathf.FloorToInt(firstPoint.x / cellSize);
        int gy = Mathf.FloorToInt(firstPoint.y / cellSize);
        gridArray[gx, gy] = firstPoint;

        while (grid.Count > 0 && positions.Count < treeCount)
        {
            int idx = Random.Range(0, grid.Count);
            Vector2 point = grid[idx];
            bool found = false;

            for (int i = 0; i < poissonAttempts; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2);
                float radius = Random.Range(minTreeDistance, minTreeDistance * 2);
                Vector2 newPoint = point + new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );

                if (newPoint.x >= 0 && newPoint.x < areaSize.x &&
                    newPoint.y >= 0 && newPoint.y < areaSize.y)
                {
                    int ngx = Mathf.FloorToInt(newPoint.x / cellSize);
                    int ngy = Mathf.FloorToInt(newPoint.y / cellSize);

                    if (IsValidPoint(newPoint, gridArray, cellSize, gridW, gridH))
                    {
                        grid.Add(newPoint);
                        gridArray[ngx, ngy] = newPoint;

                        Vector3 worldPos = GetWorldPosition(newPoint);
                        if (worldPos != Vector3.zero)
                        {
                            positions.Add(worldPos);
                            found = true;
                        }
                        break;
                    }
                }
            }

            if (!found)
            {
                grid.RemoveAt(idx);
            }
        }

        return positions;
    }

    private bool IsValidPoint(Vector2 point, Vector2[,] grid, float cellSize, int w, int h)
    {
        int gx = Mathf.FloorToInt(point.x / cellSize);
        int gy = Mathf.FloorToInt(point.y / cellSize);

        int startX = Mathf.Max(0, gx - 2);
        int endX = Mathf.Min(w - 1, gx + 2);
        int startY = Mathf.Max(0, gy - 2);
        int endY = Mathf.Min(h - 1, gy + 2);

        for (int i = startX; i <= endX; i++)
        {
            for (int j = startY; j <= endY; j++)
            {
                Vector2 neighbor = grid[i, j];
                if (neighbor.x >= 0 && Vector2.Distance(point, neighbor) < minTreeDistance)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private List<Vector3> GenerateRandomPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        for (int i = 0; i < treeCount; i++)
        {
            Vector2 randomPos = new Vector2(
                Random.Range(0, areaSize.x),
                Random.Range(0, areaSize.y)
            );

            Vector3 worldPos = GetWorldPosition(randomPos);
            if (worldPos != Vector3.zero)
            {
                positions.Add(worldPos);
            }
        }

        return positions;
    }

    private Vector3 GetWorldPosition(Vector2 localPos)
    {
        if (createCenterClearing)
        {
            Vector2 centerPoint = new Vector2(areaSize.x / 2, areaSize.y / 2) + clearingCenter;
            if (Vector2.Distance(localPos, centerPoint) < clearingRadius)
            {
                return Vector3.zero;
            }
        }

        Vector3 worldPos = new Vector3(
            localPos.x + areaOffset.x - areaSize.x / 2,
            1000f,
            localPos.y + areaOffset.y - areaSize.y / 2
        );

        if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, 2000f, groundLayer))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private void SpawnTrees(List<Vector3> positions)
    {
        foreach (Vector3 pos in positions)
        {
            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

            Quaternion baseRotation = Quaternion.Euler(rotationOffset);
            Quaternion randomYRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Quaternion finalRotation = randomYRotation * baseRotation;

            GameObject tree = Instantiate(prefab, pos, finalRotation, treeParent);

            float scale = Random.Range(minScale, maxScale);
            tree.transform.localScale = Vector3.one * scale;

            // DO NOT set isStatic for harvestable trees - they need to be destroyed
            // Only set static if NOT using mesh combining (trees won't be harvested)
            if (!useCombineMeshes && !useStaticBatching)
            {
                // Keep trees dynamic so they can be destroyed
                tree.isStatic = false;
            }
            else
            {
                tree.isStatic = true;
            }

            // Add Tree component if not already on prefab
            Tree treeComponent = tree.GetComponent<Tree>();
            if (treeComponent == null)
            {
                treeComponent = tree.AddComponent<Tree>();
            }

            spawnedTrees.Add(tree);
        }
    }

    private void CombineMeshesByMaterial()
    {
        Dictionary<Material, List<CombineInstance>> materialMeshes = new Dictionary<Material, List<CombineInstance>>();

        foreach (GameObject tree in spawnedTrees)
        {
            MeshFilter[] meshFilters = tree.GetComponentsInChildren<MeshFilter>();
            MeshRenderer[] renderers = tree.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh == null) continue;

                if (!meshFilters[i].sharedMesh.isReadable)
                {
                    Debug.LogWarning($"Mesh '{meshFilters[i].sharedMesh.name}' is not readable. Enable Read/Write in import settings or disable mesh combining.");
                    continue;
                }

                Material mat = renderers[i].sharedMaterial;

                if (!materialMeshes.ContainsKey(mat))
                {
                    materialMeshes[mat] = new List<CombineInstance>();
                }

                CombineInstance ci = new CombineInstance
                {
                    mesh = meshFilters[i].sharedMesh,
                    transform = meshFilters[i].transform.localToWorldMatrix
                };

                materialMeshes[mat].Add(ci);
            }
        }

        if (materialMeshes.Count == 0)
        {
            Debug.LogError("No meshes could be combined. Make sure your tree meshes have Read/Write enabled in import settings!");
            return;
        }

        foreach (var kvp in materialMeshes)
        {
            CreateCombinedMesh(kvp.Key, kvp.Value);
        }

        foreach (GameObject tree in spawnedTrees)
        {
            Destroy(tree);
        }
        spawnedTrees.Clear();
    }

    private void CreateCombinedMesh(Material mat, List<CombineInstance> combines)
    {
        if (combines.Count == 0) return;

        int chunkCount = Mathf.CeilToInt((float)combines.Count / maxTreesPerChunk);

        for (int chunk = 0; chunk < chunkCount; chunk++)
        {
            int start = chunk * maxTreesPerChunk;
            int count = Mathf.Min(maxTreesPerChunk, combines.Count - start);

            CombineInstance[] chunkCombines = combines.GetRange(start, count).ToArray();

            GameObject combined = new GameObject($"Combined_{mat.name}_{chunk}");
            combined.transform.parent = treeParent;
            combined.isStatic = true;

            MeshFilter mf = combined.AddComponent<MeshFilter>();
            MeshRenderer mr = combined.AddComponent<MeshRenderer>();

            Mesh combinedMesh = new Mesh();
            combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            try
            {
                combinedMesh.CombineMeshes(chunkCombines, true, true);
                mf.mesh = combinedMesh;
                mr.sharedMaterial = mat;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to combine meshes: {e.Message}");
                DestroyImmediate(combined);
            }
        }
    }

    public void ClearTrees()
    {
        if (treeParent != null)
        {
            DestroyImmediate(treeParent.gameObject);
        }

        foreach (GameObject tree in spawnedTrees)
        {
            if (tree != null)
            {
                DestroyImmediate(tree);
            }
        }

        spawnedTrees.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = new Vector3(areaOffset.x, 0, areaOffset.y);
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, 1, areaSize.y));

        if (createCenterClearing)
        {
            Gizmos.color = Color.red;
            Vector3 clearingWorldPos = new Vector3(
                areaOffset.x + clearingCenter.x,
                0,
                areaOffset.y + clearingCenter.y
            );
            DrawCircle(clearingWorldPos, clearingRadius, 32);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}