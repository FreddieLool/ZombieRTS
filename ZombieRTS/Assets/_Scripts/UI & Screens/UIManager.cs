using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using static AudioManager;
using System.Linq;


/// <summary>
/// TO DO:: Decouple any building/unit logic from UI. Spawning units and closing the unit panel postpones their spawn until its opened back again.
/// </summary>

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // Building UI
    [SerializeField] GameObject buildingUI;
    [SerializeField] private RectTransform buildingUIRectTransform; 
    private bool isBuildingUIVisible = false;
    public RectTransform contentPanel; // Assign this in the inspector
    public GridLayoutGroup gridLayoutGroup;
    public ScrollRect scrollRect;

    // ----------------

    [SerializeField] private BuildingData[] buildings; // all buildings
    // DECOUPLE buulding data

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform WorldSpaceCanvasParent;
    [SerializeField] private Transform buttonParent;
    [SerializeField] private TextMeshProUGUI cycleCounterText;


    // Resource Stats
    [SerializeField] private TextMeshProUGUI boneResourcesTxt;
    [SerializeField] private TextMeshProUGUI biohazardResourcesTxt;
    [SerializeField] private TextMeshProUGUI fleshResourcesTxt;


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

    // Unit construction representation prefab stuff
    public RectTransform unitButtonContentPanel; // Assign in the Unity Inspector
    public HorizontalLayoutGroup horizontalLayoutGroup;


    void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
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
        ResourceManager.OnResourcesUpdated += UpdateResourceUI; // Subscribe to updates
        UpdateResourceUI(); // Initial update
        ClearExistingUnitUI(); // clean any
    }

    void Update()
    {
        HandleBuildingUIToggle();
        HandleUnitPanelToggle();

        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerOverUIObject())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    Building building = hit.collider.GetComponent<Building>();
                    if (building != null && building.Selectable)
                    {
                        ShowBuildingInfo(building); // Show UI for the clicked building
                        return; // Prevent further UI hiding
                    }
                }

                //HideUI(); // Only hide UI if not clicking on any part of the UI
            }
            else if(IsPointerOverUIObject() && Input.GetMouseButtonDown(0))
            {
                Debug.Log("Click inside UI!");
            }
        }
    }

    public void UpdateResourceUI()
    {
        if (ResourceManager.Instance != null)
        {
            TweenResourceValue(boneResourcesTxt, ResourceManager.Instance.GetResourceAmount("Bone"));
            TweenResourceValue(biohazardResourcesTxt, ResourceManager.Instance.GetResourceAmount("Biohazard"));
            TweenResourceValue(fleshResourcesTxt, ResourceManager.Instance.GetResourceAmount("Flesh"));
        }
    }

    private void TweenResourceValue(TextMeshProUGUI resourceText, int targetValue)
    {
        int currentValue = int.TryParse(resourceText.text, out currentValue) ? currentValue : 0;
        LeanTween.value(gameObject, currentValue, targetValue, 1.5f)
            .setOnUpdate((float value) => {
                resourceText.text = Mathf.FloorToInt(value).ToString();
            })
            .setEase(LeanTweenType.easeInOutQuad);
    }

    void OnDestroy()
    {
        ResourceManager.OnResourcesUpdated -= UpdateResourceUI; // Unsubscribe to avoid memory leaks
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
        ClearExistingUnitButtons(); // Remove existing buttons

        float totalWidth = 0;
        GameObject tempButton = Instantiate(unitButtonPrefab, unitButtonParent);
        RectTransform btnRect = tempButton.GetComponent<RectTransform>();
        float buttonWidth = btnRect.rect.width;
        Destroy(tempButton);

        // Calculate total width based on the button widths and the defined spacing and padding in the Horizontal Layout Group
        totalWidth = units.Count() * buttonWidth;
        if (units.Count() > 1)
        {
            totalWidth += (units.Count() - 1) * unitButtonParent.GetComponent<HorizontalLayoutGroup>().spacing;
        }
        totalWidth += unitButtonParent.GetComponent<HorizontalLayoutGroup>().padding.left + unitButtonParent.GetComponent<HorizontalLayoutGroup>().padding.right;

        // Set the width of the content panel
        unitButtonContentPanel.sizeDelta = new Vector2(totalWidth, unitButtonContentPanel.sizeDelta.y);

        foreach (var unit in units)
        {
            GameObject btnObj = Instantiate(unitButtonPrefab, unitButtonParent);
            UnitButton unitButton = btnObj.GetComponent<UnitButton>();
            unitButton.Setup(unit);
        }
    }

    private void ClearExistingUnitButtons()
    {
        foreach (Transform child in unitButtonParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void OnUnitButtonClicked(UnitData unitData)
    {
        if (ResourceManager.Instance.HasEnoughResources(unitData.costs))
        {
            ResourceManager.Instance.DeductResources(unitData.costs); // Ensure this happens ONLY here
            StartUnitConstruction(unitData);
        }
        else
        {
            Debug.Log("Not enough resources!");
            AudioManager.Instance.PlaySoundEffect(SoundEffect.ErrorClick);
        }
    }

    public void StartUnitConstruction(UnitData unitData)
    {
        Debug.Log("Unit Construction started!");
        GameObject uiObj = Instantiate(unitConstructionPrefab, unitConstructionParent);
        uiObj.GetComponent<UnitConstructionUI>().Initialize(unitData.icon, unitData.buildTime, unitData);
    }

    private void ClearExistingUnitUI()
    {
        foreach (Transform child in unitConstructionParent)
        {
            Destroy(child.gameObject);
        }
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

    public bool IsPointerOverUIObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("BuildingUI"))
            {
                return true;
            }
        }

        return false;
    }


    // Helper method to check if the GameObject is part of the Building UI hierarchy
    private bool IsPartOfBuildingUI(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (parent.gameObject.CompareTag("BuildingUI"))
                return true;
            parent = parent.parent;
        }
        return false;
    }


    private void PopulateBuildingButtons()
    {
        ClearExistingButtons(); // destroy existing buttons

        int numberOfBuildings = buildings.Count();
        int numberOfColumns = gridLayoutGroup.constraintCount;
        float rowHeight = gridLayoutGroup.cellSize.y + gridLayoutGroup.spacing.y;
        int numberOfRows = Mathf.CeilToInt((float)numberOfBuildings / numberOfColumns);

        // Calculate the required height of the content
        float requiredHeight = numberOfRows * rowHeight + gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom - gridLayoutGroup.spacing.y;

        // Set the size of the content panel
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, requiredHeight);

        foreach (var building in buildings)
        {
            GameObject btnObj = Instantiate(buttonPrefab, contentPanel.transform, false);
            BuildingButton buildingButton = btnObj.GetComponent<BuildingButton>();
            buildingButton.Setup(building);
            BuildingData currentBuilding = building; // temp var to correctly capture the current loop iteration
            buildingButton.button.onClick.AddListener(() => SelectBuildingButton(currentBuilding));
        }

        // Reset the scroll position to the top after populating
        scrollRect.verticalNormalizedPosition = 1.0f;
    }

    private void ClearExistingButtons()
    {
        foreach (Transform child in contentPanel.transform)
        {
            Destroy(child.gameObject);
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
        BuildingUI buildingUI = activeInfoUI.GetComponent<BuildingUI>();
        if (buildingUI != null)
        {
            buildingUI.SetupUI(building);
        }
        else
        {
            Debug.LogError("BuildingUI component is missing on the prefab!");
        }
    }

    public void HideUI()
    {
        if (activeInfoUI != null)
        {
            Debug.Log("Hiding UI now");
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

    public void ResetSaveData()
    {
        Debug.Log("Resetting all save data...");
        SaveSystem.ResetSaveData();  // You will need to implement this method in your SaveSystem class
    }

    public void HackGameNow()
    {
        GameManager.Instance.HaxResources();
    }

    private void ShowBuildingUI()
    {
        buildingUI.SetActive(true);
        isBuildingUIVisible = true;
        // Start from the right off-screen
        Vector3 startPosition = new Vector3(Screen.width, buildingUIRectTransform.anchoredPosition.y, 0);
        Vector3 endPosition = new Vector3(0, buildingUIRectTransform.anchoredPosition.y, 0);  // it fits at x=0 when fully visible

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

}
