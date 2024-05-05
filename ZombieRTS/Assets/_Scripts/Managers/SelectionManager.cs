using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AudioManager;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool isDragging = false;

    private List<GameObject> selectedUnits = new List<GameObject>();
    private GameObject selectedBuilding;
    private bool isAttackMode = false;
    [SerializeField] public Texture2D attackModeCursorTexture;
    public static SelectionManager Instance { get; private set; }

    void Awake()
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

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    // XXXXXXXXX
    // XXXXXXXXXX
    // XXXXXXXXXXXXXXXXXXXX BACKUP

    void Update()
    {
        HandleMouseInput();
        if (isDragging)
        {
            endPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            UpdateSelectionInBox(); // Update the selection in real-time
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            StartAttackMode();
        }
    }

    void OnGUI()
    {
        if (isDragging)
        {
            var rect = GetScreenRect(startPosition, endPosition);
            DrawScreenRect(rect, new Color(1f, 0f, 0f, 0.25f)); 
            DrawScreenRectBorder(rect, 2, Color.red);
        }
    }


    private void StartAttackMode()
    {
        Debug.Log("Attack mode active. Click on an enemy to attack.");
    }

    private void HandleAttackCommand(Vector3 point)
    {
        // Assume this is called when the player clicks on an enemy while in attack mode
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(point), out hit))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                foreach (var selected in selectedUnits)
                {
                    selected.GetComponent<UnitController>().SetTarget(hit.transform);
                }
            }
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            bool isCtrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            startPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {

                // attack mode?
                if (/*isAttackMode && */ hit.collider.CompareTag("Enemy"))
                {
                    // Command all selected units to attack this enemy
                    foreach (var selected in selectedUnits)
                    {
                        selected.GetComponent<UnitController>().SetTarget(hit.transform);
                    }
                    isAttackMode = false;  // Reset attack mode after command
                }




                if (hit.collider.CompareTag("Unit"))
                {
                    GameObject hitUnit = hit.collider.gameObject;
                    if (selectedUnits.Contains(hitUnit) && isCtrlHeld)
                    {
                        DeselectUnit(hitUnit);
                    }
                    else if (!selectedUnits.Contains(hitUnit) && isCtrlHeld)
                    {
                        SelectUnit(hitUnit);
                    }
                    else
                    {
                        DeselectAll();
                        SelectUnit(hitUnit);
                    }
                }




                if (hit.collider.CompareTag("Building"))
                {
                    SelectBuilding(hit.collider.gameObject);
                }
                else if (!UIManager.Instance.IsPointerOverUIObject())  // THE FUCKING WORKAROUND TO DETECT CLICK ON UI AND NOT DESELECT THE BUILDING!!!!!!!
                {
                    DeselectAll();
                }


            }
            else if (!UIManager.Instance.IsPointerOverUIObject())
            {
                DeselectAll();
            }
            else
            {
                if (!isCtrlHeld) DeselectAll();
            }

            isDragging = false;
        }

        if (Input.GetMouseButton(0))
        {
            if (!isDragging && Vector2.Distance(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y), startPosition) > 10f)
            {
                isDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            SelectUnitsInBox();
            isDragging = false;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // Toggle attack mode
            isAttackMode = !isAttackMode;
            Debug.Log("Attack mode " + (isAttackMode ? "enabled" : "disabled"));
        }
    }

    void UpdateCursorForAttackMode()
    {
        if (isAttackMode)
        {
            Cursor.SetCursor(attackModeCursorTexture, Vector2.zero, CursorMode.Auto);  // Set an attack cursor
        }
        else
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);  // Reset cursor to default
        }
    }

    void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
    }

    void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Draw top
        GUI.color = color;
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, thickness), Texture2D.whiteTexture);
        // Draw left
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
        // Draw right
        GUI.DrawTexture(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), Texture2D.whiteTexture);
        // Draw bottom
        GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), Texture2D.whiteTexture);
    }

    Rect GetScreenRect(Vector2 screenPosition1, Vector2 screenPosition2)
    {
        // Calculate directly using GUI-ready coordinates
        float xMin = Mathf.Min(screenPosition1.x, screenPosition2.x);
        float xMax = Mathf.Max(screenPosition1.x, screenPosition2.x);
        float yMin = Mathf.Min(screenPosition1.y, screenPosition2.y);
        float yMax = Mathf.Max(screenPosition1.y, screenPosition2.y);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private void SelectBuilding(GameObject building)
    {
        if (BuildingManager.Instance.isPlacingBuilding) return; // Ignore if placing a building

        Building buildingComponent = building.GetComponent<Building>();
        if (buildingComponent != null && buildingComponent.Selectable)
        {
            if (selectedBuilding != building)
            {
                DeselectAll();  // Deselect any previously selected objects
                selectedBuilding = building;
                buildingComponent.DisplayInfo();
                StartSelectionPopAnimation(building.transform); 
                AudioManager.Instance.PlaySoundEffect(SoundEffect.ClickOnBuilding); 
            }
            // Do nothing if the building is already selected, thus not playing the sound again
        }
    }

    // Building click animation
    private void StartSelectionPopAnimation(Transform buildingTransform)
    {
        Vector3 originalScale = buildingTransform.localScale;

        LeanTween.scale(buildingTransform.gameObject, originalScale * 1.05f, 0.15f).setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => {
                LeanTween.scale(buildingTransform.gameObject, originalScale, 0.1f).setEase(LeanTweenType.easeInQuad);
            });
    }

    private void DeselectBuilding()
    {
        if (selectedBuilding != null)
        {
            Building buildingComponent = selectedBuilding.GetComponent<Building>();
            if (buildingComponent != null)
            {
                buildingComponent.HideInfo();
            }
            selectedBuilding = null;
        }
    }

    public GameObject GetSelectedBuilding()
    {
        if (!selectedBuilding) return null;
        
        return  selectedBuilding;
    }

    // Retrieve the currently selected units as UnitController instances
    public List<UnitController> GetSelectedUnitControllers()
    {
        return selectedUnits.Select(unit => unit.GetComponent<UnitController>()).ToList();
    }

    private void SelectUnit(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            UnitController unitController = unit.GetComponent<UnitController>();
            if (unitController != null)
            {
                unitController.ToggleSelection(true);
                Unit unitComponent = unitController.GetComponent<Unit>();
                if (unitComponent != null)
                {
                    UnitInfoDisplay.Instance.UpdateUnitInfo(unitComponent);
                }
            }

            //AudioManager.Instance.PlayRandomUnitSelectedSound();

            // change this
            AudioManager.Instance.PlaySoundEffect(SoundEffect.ClickOnBuilding);
        }

        UnitInfoDisplay.Instance.UpdateSelectedUnitsCount(selectedUnits.Count);
    }

    private void DeselectUnit(GameObject unit)
    {
        if (selectedUnits.Contains(unit))
        {
            selectedUnits.Remove(unit);
            UnitController unitController = unit.GetComponent<UnitController>();
            if (unitController != null)
            {
                unitController.ToggleSelection(false);
            }
        }

        if (selectedUnits.Count == 0)
        {
            UnitInfoDisplay.Instance.ClearInfo();
        }
        UnitInfoDisplay.Instance.UpdateSelectedUnitsCount(selectedUnits.Count);
    }

    private void SelectUnitsInBox()
    {
        Rect screenRect = GetScreenRect(startPosition, endPosition);
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            Vector3 unitScreenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
            unitScreenPos.y = Screen.height - unitScreenPos.y; // Convert to GUI coordinates
            if (screenRect.Contains(unitScreenPos))
            {
                SelectUnit(unit);
            }
            UnitInfoDisplay.Instance.UpdateSelectedUnitsCount(selectedUnits.Count);
        }
    }

    private void DeselectAll()
    {
        if (selectedBuilding != null)
        {
            DeselectBuilding();
        }

        foreach (GameObject unit in selectedUnits)
        {
            UnitController unitController = unit.GetComponent<UnitController>();
            if (unitController != null)
            {
                unitController.ToggleSelection(false);
            }
        }
        selectedUnits.Clear();
        UnitInfoDisplay.Instance.ClearInfo();
    }

    private void UpdateSelectionInBox()
    {
        Rect screenRect = GetScreenRect(startPosition, endPosition);
        var currentlyHoveredUnits = new HashSet<GameObject>();

        // Check each unit if it's within the selection box
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (unit.layer == LayerMask.NameToLayer("ClickableUnit"))
            {
                bool isInSelectionBox = screenRect.Contains(mainCamera.WorldToScreenPoint(unit.transform.position), true);
                if (isInSelectionBox)
                {
                    currentlyHoveredUnits.Add(unit);
                    if (!selectedUnits.Contains(unit))
                    {
                        SelectUnit(unit);  // Select newly included units
                    }
                }
                UnitInfoDisplay.Instance.UpdateSelectedUnitsCount(selectedUnits.Count);
            }
        }

        // Deselect units that were selected before but now are outside the selection box
        foreach (GameObject previouslySelectedUnit in new List<GameObject>(selectedUnits))
        {
            if (!currentlyHoveredUnits.Contains(previouslySelectedUnit))
            {
                DeselectUnit(previouslySelectedUnit);
            }
        }
        UnitInfoDisplay.Instance.UpdateSelectedUnitsCount(selectedUnits.Count);
        // Update the current selection
        selectedUnits = new List<GameObject>(currentlyHoveredUnits);
    }
}

