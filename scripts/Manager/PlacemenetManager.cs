using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    // Tipos de construção
    public enum BuildingType { House, Farm, Villager }

    // Custos
    public static int houseWoodCost = 50;
    public static int farmWoodCost = 100;
    public static int villagerWoodCost = 50;  // Changed from 100 to 50
    public static int villagerFoodCost = 50;  // Changed from 100 to 50

    [Header("Prefabs")]
    public GameObject housePrefab;
    public GameObject farmPrefab;
    public GameObject villagerPrefab;

    [Header("Collision")]
    public LayerMask collisionLayer; // Camadas para checar colisão

    [Header("Preview Colors")]
    public Color validColor = new Color(0f, 1f, 0f, 0.5f);   // Verde transparente
    public Color invalidColor = new Color(1f, 0f, 0f, 0.5f); // Vermelho transparente

    GameObject previewObject;
    BuildingType placingType;
    bool isPlacing = false;
    bool canPlace = true;
    Camera cam;

    // Materiais de preview
    List<Renderer> previewRenderers = new List<Renderer>();
    List<Material> previewMaterials = new List<Material>();

    void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    void Update()
    {
        if (!isPlacing) return;

        // Não coloca se o mouse estiver sobre UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            Vector3 pos = hit.point;
            pos.y = 0f;
            previewObject.transform.position = pos;

            CheckPlacementValidity();
            UpdatePreviewColor();

            if (Input.GetMouseButtonDown(0))
            {
                if (canPlace)
                {
                    TryPlace(pos);
                }
            }
            else if (Input.GetMouseButtonDown(1))
            {
                CancelPlacement();
            }
        }
    }

    public void StartPlacing(BuildingType type)
    {
        placingType = type;

        // ========== CASAS ==========
        if (type == BuildingType.House)
        {
            if (GameManager.Instance.wood < houseWoodCost)
            {
                Debug.Log("Not enough wood");
                return;
            }
            previewObject = Instantiate(housePrefab);
        }
        // ========== FAZENDAS ==========
        else if (type == BuildingType.Farm)
        {
            if (GameManager.Instance.wood < farmWoodCost)
            {
                Debug.Log("Not enough wood");
                return;
            }
            previewObject = Instantiate(farmPrefab);
        }
        // ========== ALDEÕES ==========
        else if (type == BuildingType.Villager)
        {
            if (GameManager.Instance.wood < villagerWoodCost || GameManager.Instance.food < villagerFoodCost)
            {
                Debug.Log("Not enough resources! Need 50 wood and 50 food.");  // Updated message
                return;
            }
            previewObject = Instantiate(villagerPrefab);
        }

        // Desativa colisores no preview
        var colliders = previewObject.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        SetupPreviewMaterials(previewObject);
        isPlacing = true;
        UIManager.Instance.CloseShop();
    }

    void SetupPreviewMaterials(GameObject obj)
    {
        previewRenderers.Clear();
        previewMaterials.Clear();

        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            previewRenderers.Add(r);

            Material mat = new Material(r.sharedMaterial);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            r.material = mat;
            previewMaterials.Add(mat);
        }
    }

    void CheckPlacementValidity()
    {
        Bounds bounds = GetCombinedBounds(previewObject);

        Collider[] overlaps = Physics.OverlapBox(
            bounds.center,
            bounds.extents,
            previewObject.transform.rotation,
            collisionLayer
        );

        canPlace = overlaps.Length == 0;
    }

    Bounds GetCombinedBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(obj.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    void UpdatePreviewColor()
    {
        Color targetColor = canPlace ? validColor : invalidColor;

        foreach (var mat in previewMaterials)
        {
            mat.color = targetColor;
        }
    }

    void TryPlace(Vector3 pos)
    {
        // ========== CASA ==========
        if (placingType == BuildingType.House)
        {
            if (!GameManager.Instance.SpendWood(houseWoodCost))
            {
                Debug.Log("Not enough wood");
                return;
            }

            Instantiate(housePrefab, pos, Quaternion.identity);
            FinishPlacement();
        }
        // ========== FAZENDA ==========
        else if (placingType == BuildingType.Farm)
        {
            if (!GameManager.Instance.SpendWood(farmWoodCost))
            {
                Debug.Log("Not enough wood");
                return;
            }

            var go = Instantiate(farmPrefab, pos, Quaternion.identity);
            var f = go.GetComponent<Farm>();
            if (f != null) GameManager.Instance.farms.Add(f);
            FinishPlacement();
        }
        // ========== ALDEÃO ==========
        else if (placingType == BuildingType.Villager)
        {
            // Verifica se há recursos
            if (GameManager.Instance.wood < villagerWoodCost || GameManager.Instance.food < villagerFoodCost)
            {
                Debug.Log("Not enough resources! Need 50 wood and 50 food.");  // Updated message
                return;
            }

            // Remove recursos
            GameManager.Instance.wood -= villagerWoodCost;
            GameManager.Instance.food -= villagerFoodCost;

            // Cria aldeão
            Instantiate(villagerPrefab, pos, Quaternion.identity);
            Debug.Log("Villager placed! -50 wood, -50 food");  // Updated message
            FinishPlacement();
        }
    }

    void FinishPlacement()
    {
        Destroy(previewObject);
        previewObject = null;
        previewRenderers.Clear();
        previewMaterials.Clear();
        isPlacing = false;
    }

    void CancelPlacement()
    {
        Destroy(previewObject);
        previewObject = null;
        previewRenderers.Clear();
        previewMaterials.Clear();
        isPlacing = false;
    }
}