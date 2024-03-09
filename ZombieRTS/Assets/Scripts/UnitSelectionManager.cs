using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitSelectionManager : MonoBehaviour
{
    public LayerMask Clickable;
    public LayerMask Ground;
    public GameObject GroundMarker;
    public List<GameObject> selectedUnits = new List<GameObject>();
    public List<GameObject> allUnitsList = new List<GameObject>();

    Camera mainCamera;

    public static UnitSelectionManager Instance { get; set; }

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

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Clickable))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    MultiSelect(hit.collider.gameObject);
                }
                else
                {
                    SelectByClicking(hit.collider.gameObject);
                }
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                {
                    DeselectAll();

                }
            }
        }
        if (Input.GetMouseButtonDown(1) && selectedUnits.Count > 0)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Ground))
            {
                GroundMarker.transform.position = hit.point;
                GroundMarker.SetActive(false);

                GroundMarker.SetActive(true);
            }
        }
    }
    void SelectByClicking(GameObject selectedUnit)
    {
        DeselectAll();

        selectedUnits.Add(selectedUnit);
        TriggerSelectionIndicator(selectedUnit, true);
        IsAbleToMove(selectedUnit, true);
    }

    public void DeselectAll()
    {
        foreach (var selectedUnit in selectedUnits)
        {
            IsAbleToMove(selectedUnit, false);
            TriggerSelectionIndicator(selectedUnit, false);
        }
        GroundMarker.SetActive(false);
        selectedUnits.Clear();
    }

    void MultiSelect(GameObject selectedUnit)
    {
        if (selectedUnits.Contains(selectedUnit) == false)
        {
            selectedUnits.Add(selectedUnit);
            TriggerSelectionIndicator(selectedUnit, true);
            IsAbleToMove(selectedUnit, true);
        }
        else
        {
            IsAbleToMove(selectedUnit, false);
            TriggerSelectionIndicator(selectedUnit, false);
            selectedUnits.Remove(selectedUnit);
        }
    }

    void IsAbleToMove(GameObject selectedUnit, bool isAbleToMove)
    {
        selectedUnit.GetComponent<UnitController>().enabled = isAbleToMove;
    }

    void TriggerSelectionIndicator(GameObject selectedUnit, bool isVisible)
    {
        selectedUnit.transform.GetChild(0).gameObject.SetActive(isVisible);
    }

    internal void DragSelect(GameObject selectedUnit)
    {
        if (selectedUnits.Contains(selectedUnit) == false)
        {
            selectedUnits.Add(selectedUnit);
            TriggerSelectionIndicator(selectedUnit, true);
            IsAbleToMove(selectedUnit, true);                                                                                                                                                                                                       
        }
    }
}
