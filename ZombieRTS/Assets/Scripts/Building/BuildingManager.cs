using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum PlacementMode
{
    Fixed,
    Valid,
    Invalid
}

public class BuildingManager : MonoBehaviour
{
    [Header("Building Setup")]
    public Building currentBuilding; // Set this when a building button is clicked (displaying)
    public LayerMask groundLayer;
    public Transform buildingParent; // Parent object to keep hierarchy organized

    [Header("Visual Feedback")]
    public Material placementMaterial; // Material based on shader that has the isValid toggle

    private GameObject toBuildInstance;
    private Camera mainCamera;
    private bool placementValid;
    private MeshRenderer[] meshRenderers;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (currentBuilding != null)
        {
            HandleBuildingPlacement();
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void HandleBuildingPlacement()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return; // Check if mouse is over UI

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (toBuildInstance == null)
            {
                toBuildInstance = Instantiate(currentBuilding.prefab, hit.point, Quaternion.identity, buildingParent);
                SetupBuildingMaterials(toBuildInstance);
            }

            toBuildInstance.transform.position = hit.point;
            placementValid = CheckPlacement(hit.point);

            UpdateMaterialValidity(placementValid);

            if (Input.GetMouseButtonDown(0) && placementValid)
            {
                FinishPlacement();
            }
        }
        else if (toBuildInstance != null)
        {
            Destroy(toBuildInstance);
        }

        if (Input.GetMouseButtonDown(1) && toBuildInstance != null) // Right clkkk to cancel
        {
            Destroy(toBuildInstance);
            AudioManager.Instance.PlayErrorSound();
        }
    }

    private void SetupBuildingMaterials(GameObject building)
    {
        meshRenderers = building.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in meshRenderers)
        {
            renderer.material = placementMaterial;
        }
    }

    // 1 Material works on shader now, with logic inside it to trigger 2 colors (S_Placement)
    private void UpdateMaterialValidity(bool isValid)
    {
        placementMaterial.SetFloat("IsValid", isValid ? 1f : 0f);
    }

    private void FinishPlacement()
    {
        AudioManager.Instance.PlayOneShotSFX(currentBuilding.placementSound);
        toBuildInstance = null; // Reset for next placement
        currentBuilding = null; // Clear the current building selection
    }

    public void SelectBuilding(Building building)
    {
        currentBuilding = building; // Set by UI button
    }

    private bool CheckPlacement(Vector3 position)
    {
        // TODO: More checks (trees, trunks, hills, height, etc..)
        return !Physics.CheckSphere(position, 1f, groundLayer);
    }
}