using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelectionManager : MonoBehaviour
{
    [SerializeField] private NavMeshAgent playerNavMeshAgent;
    [SerializeField] private LayerMask selectableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask attackableLayer;
    [SerializeField] private GameObject groundMarker;

    private List<GameObject> selectedUnits = new List<GameObject>();
    public List<GameObject> allUnits = new List<GameObject>();
    private Camera mainCamera;
    private Coroutine groundMarkerCoroutine;

    public static UnitSelectionManager Instance { get; private set; }

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
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMouseInput();
    }

    // Detects and processes mouse input
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ProcessLeftClick();
        }
        if (Input.GetMouseButtonDown(1))
        {
            ProcessRightClick();
        }
    }

    // Handles selection logic on left mouse button click
    private void ProcessLeftClick()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
        {
            bool isMultiSelect = Input.GetKey(KeyCode.LeftShift);
            SelectUnit(hit.collider.gameObject, isMultiSelect);
        }
        else if (!Input.GetKey(KeyCode.LeftShift))
        {
            DeselectAll();
        }
    }

    // Handles action logic on right mouse button click
    private void ProcessRightClick()
    {
        if (selectedUnits.Count == 0) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            PositionGroundMarker(hit.point);
        }

        if (selectedUnits.Exists(unit => unit.GetComponent<AttackController>()) && Physics.Raycast(ray, out hit, Mathf.Infinity, attackableLayer))
        {
            AssignTargets(hit.transform);
        }
    }

    // Sets the position of the ground marker and initiates its animation
    private void PositionGroundMarker(Vector3 hitPoint)
    {
        Vector3 markerPosition = hitPoint + Vector3.up;
        groundMarker.transform.position = markerPosition;
        groundMarker.SetActive(true);

        if (groundMarkerCoroutine != null)
        {
            StopCoroutine(groundMarkerCoroutine);
            groundMarker.transform.localScale = Vector3.one;
        }

        groundMarkerCoroutine = StartCoroutine(AnimateGroundMarker());
    }

    // Coroutine to animate the ground marker's disappearance
    IEnumerator AnimateGroundMarker()
    {
        yield return new WaitForSeconds(5f);

        float duration = 0.5f;
        Vector3 startScale = groundMarker.transform.localScale;
        Vector3 endScale = Vector3.zero;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            groundMarker.transform.localScale = Vector3.Lerp(startScale, endScale, t / duration);
            yield return null;
        }

        groundMarker.SetActive(false);
        groundMarker.transform.localScale = startScale;
    }

    // Selects or deselects a unit based on user input
    private void SelectUnit(GameObject unit, bool isMultiSelect)
    {
        if (!isMultiSelect)
        {
            DeselectAll();
        }

        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            ToggleUnitSelection(unit, true);
        }
    }

    // Deselects all currently selected units
    public void DeselectAll()
    {
        foreach (GameObject unit in selectedUnits)
        {
            ToggleUnitSelection(unit, false);
        }
        selectedUnits.Clear();
        groundMarker.SetActive(false);
    }

    // Assigns the specified target to selected units capable of attacking
    private void AssignTargets(Transform target)
    {
        foreach (GameObject unit in selectedUnits)
        {
            AttackController attackController = unit.GetComponent<AttackController>();
            if (attackController != null)
            {
                attackController.Target = target;
            }
        }
    }

    // Toggles the selection state of a unit
    private void ToggleUnitSelection(GameObject unit, bool isSelected)
    {
        unit.GetComponent<UnitController>().enabled = isSelected;
        GameObject selectionIndicator = unit.transform.GetChild(0).GetChild(0).gameObject;
        selectionIndicator.SetActive(isSelected);
    }

    public List<GameObject> GetAllUnits()
    {
        return allUnits;
    }

    // Adds a unit to the selected units list
    public void AddToSelectedUnits(GameObject unit)
    {
        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            ToggleUnitSelection(unit, true);
        }
    }

    // Adds a unit 2 list safely
    public void AddUnit(GameObject unit)
    {
        if (!allUnits.Contains(unit))
        {
            allUnits.Add(unit);
        }
    }

    // Removes a unit safely
    public void RemoveUnit(GameObject unit)
    {
        if (allUnits.Contains(unit))
        {
            allUnits.Remove(unit);
        }
    }
}

