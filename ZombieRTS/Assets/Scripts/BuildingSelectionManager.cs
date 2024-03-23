using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class BuildingSelectionManager : MonoBehaviour

{
    [SerializeField] GameObject ConstructionUI;
    [SerializeField] MouseOverUI MouseOverUI;
    public LayerMask Clickable;
    public List<GameObject> allBuildingsList = new List<GameObject>();
    public GameObject SelectedBuilding;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && allBuildingsList.Count > 0)
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, Clickable))
            {
                SelectByClicking(hit.collider.gameObject);
            }
            else if (!MouseOverUI.IsMouseOver())
            {
                Deselect();
            }
        }
    }

    private void Deselect()
    {
        foreach (var selectedBuilding in allBuildingsList)
        {
            ConstructionUI.SetActive(false);
            TriggerSelectionIndicator(selectedBuilding, false);
        }


    }

    private void SelectByClicking(GameObject selectedBuilding)
    {
        Deselect();

        SelectedBuilding = selectedBuilding;

        TriggerSelectionIndicator(selectedBuilding, true);
        ConstructionUI.SetActive(true);
    }

    void TriggerSelectionIndicator(GameObject selectedBuilding, bool isVisible)
    {
        selectedBuilding.transform.GetChild(0).gameObject.SetActive(isVisible);

    }
}
