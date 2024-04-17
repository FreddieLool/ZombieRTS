using System.Collections.Generic;
using UnityEngine;


public class BuildingSelectionManager : MonoBehaviour

{
    [SerializeField] GameObject constructionUI;
    [SerializeField] MouseOverUI mouseOverUI;
    public LayerMask clickable;
    public List<GameObject> allBuildingsList = new List<GameObject>();
    public GameObject selectedBuilding;
    private Camera mainCamera;

    public static BuildingSelectionManager Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main camera not found!");
        }
    }

    // Checks for user input to select or deselect buildings.
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && allBuildingsList.Count > 0)
        {
            HandleBuildingSelection();
        }
    }

    private void HandleBuildingSelection()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickable))
        {
            SelectByClicking(hit.collider.gameObject);
        }
        else if (mouseOverUI != null && !mouseOverUI.IsMouseOver())
        {
            DeselectAllBuildings();
        }
    }

    // Deselects all buildings and hides related UI components
    private void DeselectAllBuildings()
    {
        foreach (var building in allBuildingsList)
        {
            TriggerSelectionIndicator(building, false);
        }
        if (constructionUI != null)
        {
            constructionUI.SetActive(false);
        }
    }

    // Handles selection logic when a building is clicked
    private void SelectByClicking(GameObject building)
    {
        DeselectAllBuildings();

        selectedBuilding = building;
        TriggerSelectionIndicator(building, true);
        constructionUI.SetActive(true);
    }

    // Toggles the selection indicator for a building
    private void TriggerSelectionIndicator(GameObject building, bool isVisible)
    {
        // Assuming the first child is ALWAYS the selection indicator
        building.transform.GetChild(0).gameObject.SetActive(isVisible);
    }
}

