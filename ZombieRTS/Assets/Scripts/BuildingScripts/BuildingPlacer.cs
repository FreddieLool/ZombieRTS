using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacer : MonoBehaviour
{
    [SerializeField] private BuildingSelectionManager buildingSelectionManager;
    public static BuildingPlacer Instance { get; private set; }
    public LayerMask groundLayer;

    private GameObject buildingPrefab;
    private GameObject toBuild;
    private Camera mainCamera;

    private void Awake()
    {
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleBuildingPlacement();
    }

    // Handles all building placement logic & input checks
    private void HandleBuildingPlacement()
    {
        if (buildingPrefab == null) return;

        HandleRightClickCancellation();
        ToggleActiveBuildingBasedOnUI();
        RotateBuildingOnInput();
        PlaceBuildingOnGround();
    }

    // Destroys the building object if right-clicked to cancel placement
    private void HandleRightClickCancellation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Destroy(toBuild);
            buildingPrefab = null;
            toBuild = null;
        }
    }

    // Disables building object when the mouse is over UI elements.
    private void ToggleActiveBuildingBasedOnUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            if (toBuild.activeSelf) toBuild.SetActive(false);
        }
        else if (!toBuild.activeSelf)
        {
            toBuild.SetActive(true);
        }
    }

    // Allows rotation of the building objects
    private void RotateBuildingOnInput()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            toBuild.transform.Rotate(Vector3.up, 90);
        }
    }

    // Handles the placement of the building object on the ground
    private void PlaceBuildingOnGround()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            if (!toBuild.activeSelf) toBuild.SetActive(true);
            toBuild.transform.position = hit.point;

            if (Input.GetMouseButtonDown(0))
            {
                BuildingManager buildingManager = toBuild.GetComponent<BuildingManager>();
                if (buildingManager.hasValidPlacement)
                {
                    FinalizePlacement(buildingManager);
                }
            }
        }
        else if (toBuild.activeSelf)
        {
            toBuild.SetActive(false);
        }
    }

    // Finalizes the placement of the building.
    private void FinalizePlacement(BuildingManager buildingManager)
    {
        buildingManager.UpdatePlacementMode(PlacementMode.Fixed);
        buildingSelectionManager.allBuildingsList.Add(toBuild);
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            PrepareBuilding();
        }
        else
        {
            buildingPrefab = null;
            toBuild = null;
        }
    }

    // Sets the building prefab to use for placement
    public void SetBuildingPrefab(GameObject prefab)
    {
        buildingPrefab = prefab;
        PrepareBuilding();
        EventSystem.current.SetSelectedGameObject(null);
    }

    // Prepares a new building for placement
    private void PrepareBuilding()
    {
        if (toBuild) Destroy(toBuild);
        toBuild = Instantiate(buildingPrefab);
        toBuild.SetActive(true);

        BuildingManager buildingManager = toBuild.GetComponent<BuildingManager>();
        buildingManager.isFixed = false;
        buildingManager.UpdatePlacementMode(PlacementMode.Valid);
    }
}
