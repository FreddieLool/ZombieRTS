using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    public LayerMask Ground;
    [SerializeField] GameObject BuildingPrefab;
    [SerializeField] GameObject ToBuild;

    Camera mainCamera;
    Ray ray;
    RaycastHit hit;

    public static BuildingPlacer Instance { get; set; }

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

            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (ToBuild.activeSelf)
                {
                    ToBuild.SetActive(false);
                }
            }
            else if (!ToBuild.activeSelf)
            {
                ToBuild.SetActive(true);
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
                    if (buildingManager.hasValidPlacement)
                    {
                        buildingManager.SetPlacementMode(PlacementMode.Fixed);

                        BuildingPrefab = null;
                        ToBuild = null;
                    }
                }
            }
            else if (ToBuild.activeSelf)
            {
                ToBuild.SetActive(false);
            }
        }
    }
    public void SetBuildingPrefab(GameObject prefab)
    {
        BuildingPrefab = prefab;
        PrepareBuilding();
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
