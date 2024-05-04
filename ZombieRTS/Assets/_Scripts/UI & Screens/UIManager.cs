using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using static AudioManager;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Building UI
    [SerializeField] GameObject buildingUI;
    [SerializeField] private RectTransform buildingUIRectTransform; 
    private bool isBuildingUIVisible = false; 

    // ----------------
    


    [SerializeField] private BuildingData[] buildings; // all buildings
    // DECOUPLE buulding data

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform WorldSpaceCanvasParent;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TextMeshProUGUI cycleCounterText;


    // Building Info UI
    private Transform target; // The building's transform
    private Vector3 offset = new Vector3(0, 5, 0); 
    [SerializeField] private GameObject infoUIPrefab; // The world-space canvas prefab
    private GameObject activeInfoUI; // Currently active UI instance


    // Unit Panel
    public Transform unitConstructionParent;  // Parent object for unit construction UIs
    public GameObject unitConstructionPrefab;
    [SerializeField] private UnitData[] units; // Array of all units
    [SerializeField] private GameObject unitButtonPrefab; // Prefab for the unit buttons
    [SerializeField] private Transform unitButtonParent; // Parent transform for unit buttons
    [SerializeField] private GameObject unitPanelUI; 
    [SerializeField] private RectTransform unitPanelRectTransform;
    private bool isUnitPanelVisible = false;




    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        LightingManager.Instance.onNewCycle.AddListener(UpdateCycleCounter);
        PopulateBuildingButtons();
        PopulateUnitButtons();

        cycleCounterText.text = "" + 1;
        buildingUI.SetActive(false); 
        unitPanelUI.SetActive(false);
    }


    void Update()
    {
        HandleBuildingUIToggle();
        HandleUnitPanelToggle();

        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIObject())
        {
            // If we clicked and it's not on a UI object, then perform additional checks
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Building building = hit.collider.GetComponent<Building>();
                if (building != null)
                {
                    ShowBuildingInfo(building); // Show UI for the clicked building
                    return; 
                }
            }

            // If the raycast didn't hit a building or if it's a click in empty space, hide the UI
            HideUI();
        }
    }

    // Unit Panel

    private void HandleUnitPanelToggle()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ToggleUnitPanelUI();
        }
    }

    private void ToggleUnitPanelUI()
    {
        if (!isUnitPanelVisible)
        {
            ShowUnitPanelUI();
        }
        else
        {
            HideUnitPanelUI();
        }
    }

    private void ShowUnitPanelUI()
    {
        unitPanelUI.SetActive(true);
        isUnitPanelVisible = true;
        Vector3 startPosition = new Vector3(unitPanelRectTransform.anchoredPosition.x, -unitPanelRectTransform.rect.height, 0);
        Vector3 endPosition = Vector3.zero;

        unitPanelRectTransform.anchoredPosition = startPosition;
        LeanTween.move(unitPanelRectTransform, endPosition, 0.35f).setEase(LeanTweenType.easeOutExpo);
        AudioManager.Instance.PlaySoundEffect(SoundEffect.ShowUIBuilding);
    }

    private void HideUnitPanelUI()
    {
        Vector3 endPosition = new Vector3(unitPanelRectTransform.anchoredPosition.x, -unitPanelRectTransform.rect.height, 0);
        AudioManager.Instance.PlaySoundEffect(SoundEffect.CloseUIBuilding);
        LeanTween.move(unitPanelRectTransform, endPosition, 0.25f).setEase(LeanTweenType.easeInExpo).setOnComplete(() =>
        {
            unitPanelUI.SetActive(false);
            isUnitPanelVisible = false;
        });
    }

    private void PopulateUnitButtons()
    {
        foreach (var unit in units)
        {
            GameObject btnObj = Instantiate(unitButtonPrefab, unitButtonParent);
            UnitButton unitButton = btnObj.GetComponent<UnitButton>();
            unitButton.Setup(unit);
            UnitData currentUnit = unit;
            unitButton.button.onClick.AddListener(() => OnUnitButtonClicked(currentUnit));
        }
    }

    public void OnUnitButtonClicked(UnitData unitData)
    {
        if (ResourceManager.Instance.HasEnoughResources(unitData.costs))
        {
            ResourceManager.Instance.DeductResources(unitData.costs);
            StartUnitConstruction(unitData);
        }
        else
        {
            Debug.Log("Not enough resources!");
        }
    }

    public void StartUnitConstruction(UnitData unitData)
    {
        GameObject uiObj = Instantiate(unitConstructionPrefab, unitConstructionParent);
        uiObj.GetComponent<UnitConstructionUI>().Initialize(unitData.icon, unitData.buildTime, unitData);
    }

    private void UpdateCycleCounter()
    {
        int currentCycle = LightingManager.Instance.GetCurrentCycle();
        cycleCounterText.text = $"{currentCycle}";
        AnimateCycleText();
    }

    private void AnimateCycleText()
    {
        cycleCounterText.transform.localScale = Vector3.one * 1.25f;
        LeanTween.scale(cycleCounterText.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutElastic);
    }

    private bool IsPointerOverUIObject()
    {
        // true if the pointer is over any UI element
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void PopulateBuildingButtons()
    {
        foreach (var building in buildings)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonParent);
            BuildingButton buildingButton = btnObj.GetComponent<BuildingButton>();
            buildingButton.Setup(building);
            BuildingData currentBuilding = building; // temp var to correctly capture the current loop iteration
            buildingButton.button.onClick.AddListener(() => SelectBuildingButton(currentBuilding));
        }
    }

    private void SelectBuildingButton(BuildingData building)
    {
        Debug.Log("Building selected: " + building.buildingName);
        BuildingManager.Instance.SelectBuilding(building);
    }

    public void ShowBuildingInfo(Building building)
    {
        if (activeInfoUI != null) Destroy(activeInfoUI);

        Vector3 uiPosition = building.transform.position + new Vector3(0, 5, 0);
        activeInfoUI = Instantiate(infoUIPrefab, uiPosition, Quaternion.identity, WorldSpaceCanvasParent);
        activeInfoUI.GetComponent<BuildingUI>().SetupUI(building);
    }

    public void HideUI()
    {
        if (activeInfoUI != null)
        {
            Destroy(activeInfoUI);
        }
    }

    // Building UI Info
    private void HandleBuildingUIToggle()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildingUI();
        }
    }

    private void ToggleBuildingUI()
    {
        if (!isBuildingUIVisible)
        {
            ShowBuildingUI();
        }
        else
        {
            HideBuildingUI();
        }
    }

    private void ShowBuildingUI()
    {
        buildingUI.SetActive(true);
        isBuildingUIVisible = true;
        // Start from the right off-screen
        Vector3 startPosition = new Vector3(Screen.width, buildingUIRectTransform.anchoredPosition.y, 0);
        Vector3 endPosition = new Vector3(0, buildingUIRectTransform.anchoredPosition.y, 0);  // Assuming the UI fits at x=0 when fully visible

        buildingUIRectTransform.anchoredPosition = startPosition;
        LeanTween.move(buildingUIRectTransform, endPosition, 0.35f).setEase(LeanTweenType.easeOutExpo);
        AudioManager.Instance.PlaySoundEffect(SoundEffect.ShowUIBuilding);
    }

    private void HideBuildingUI()
    {
        Vector3 endPosition = new Vector3(Screen.width, buildingUIRectTransform.anchoredPosition.y, 0);
        AudioManager.Instance.PlaySoundEffect(SoundEffect.CloseUIBuilding);
        LeanTween.move(buildingUIRectTransform, endPosition, 0.25f).setEase(LeanTweenType.easeInExpo).setOnComplete(() =>
        {
            buildingUI.SetActive(false);
            isBuildingUIVisible = false;
        });
    }


    // UNIT UI

    public void OnUnitButtonClicked(string unitName)
    {
        MainBaseManager.Instance.SpawnUnit(unitName);
    }
}
