using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    [SerializeField] BuildingSelectionManager BuildingSelectionManager;
    public BuildingScriptableObject[] allBuildingCosts;
    public static BuildingPlacer Instance { get; set; }
    public LayerMask Ground;
    int biohazardCost;
    int bonesCost;
    GameObject BuildingPrefab;
    GameObject ToBuild;
    Camera mainCamera;

    Ray ray;
    RaycastHit hit;


    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
        BuildingPrefab = null;
    }
    private void Update()
    {
        if (BuildingPrefab != null)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Destroy(ToBuild);
                BuildingPrefab = null;
                ToBuild = null;
                return;
            }


            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (ToBuild.activeSelf)
                {
                    ToBuild.SetActive(false);
                    return;
                }
            }
            else if (!ToBuild.activeSelf)
            {
                ToBuild.SetActive(true);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ToBuild.transform.Rotate(Vector3.up, 90);
            }

            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000f, Ground))
            {
                if (!ToBuild.activeSelf)
                {
                    ToBuild.SetActive(true);

                }
                ToBuild.transform.position = hit.point;
                if (Input.GetMouseButtonDown(0))
                {
                    BuildingManager buildingManager = ToBuild.GetComponent<BuildingManager>();

                    if (buildingManager.hasValidPlacement && ResourceManager.Instance.HasEnoughResources(bonesCost, biohazardCost))
                    {
                        ResourceManager.Instance.SpendResources(bonesCost, biohazardCost);
                        buildingManager.SetPlacementMode(PlacementMode.Fixed);
                        BuildingSelectionManager.allBuildingsList.Add(ToBuild);
                        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                        {
                            ToBuild = null;
                            PrepareBuilding();
                        }
                        else
                        {
                            BuildingPrefab = null;
                            ToBuild = null;
                        }
                    }
                }
            }
            else if (ToBuild.activeSelf)
            {
                ToBuild.SetActive(false);
            }
        }
    }

    public void SetBuildingPrefab(BuildingScriptableObject buildingData)
    {
        if (buildingData.buildingPrefab != null)
        {
            BuildingPrefab = buildingData.buildingPrefab;
            bonesCost = buildingData.bonesCost;
            biohazardCost = buildingData.biohazardCost;
            PrepareBuilding();
        }
    }

    private void PrepareBuilding()
    {
        if (ToBuild)
        {
            Destroy(ToBuild);
        }
        ToBuild = Instantiate(BuildingPrefab);
        ToBuild.SetActive(true);

        BuildingManager buildingManager = ToBuild.GetComponent<BuildingManager>();
        buildingManager.isFixed = false;
        buildingManager.SetPlacementMode(PlacementMode.Valid);

    }
}
