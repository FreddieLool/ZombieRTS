using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static AudioManager;

public enum PlacementMode
{
    Fixed,
    Valid,
    Invalid
}

public class BuildingManager : MonoBehaviour
{
    public static BuildingManager Instance;

    [Header("Building Setup")]
    public BuildingData previewBuilding; // Set this when a building button is clicked (displaying)
    public LayerMask groundLayer;
    public LayerMask buildingLayer;
    public Transform buildingParent; // Parent object to keep hierarchy organized
    public GameObject player;
    private Dictionary<MeshRenderer, Material[]> originalMaterials;
    public float gridSnapSize = 2.0f;  // Grid size for snapping buildings
    public float spacing = 1.0f;  // Spacing between buildings
    public List<BuildingData> activeBuildings = new List<BuildingData>();

    [Header("Visual Feedback")]
    public Material placementMaterial; // Material based on shader that has the isValid toggle

    private GameObject toBuildInstance;
    private Camera mainCamera;
    private bool placementValid;
    private MeshRenderer[] meshRenderers;


    // grid
    public GameObject lineRendererPrefab;  // Assign in inspector
    public int gridExtent = 10;  // Determines how far the grid extends
    private List<GameObject> gridLines = new List<GameObject>();
    private List<GameObject> gridCells = new List<GameObject>();



    void Awake()
    {
        mainCamera = Camera.main;

        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && previewBuilding != null)
        {
            CancelBuildingPlacement();
        }

        if(!IsPointerOverUI())
        {
            HandleBuildingPlacement();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateBuilding();
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        return false;
    }


    // HandleBuildingPlacement method related
    Vector3 originalScale;
    bool isScalingDown = false; // Flag to control the scaling state
    private int rotationYOffset = 0; // Degrees offset by manual rotation
    Quaternion baseRotation;
    private void HandleBuildingPlacement()
    {
        if (previewBuilding == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            if (toBuildInstance == null)
            {
                Vector3 snapPosition = SnapPositionToGrid(hit.point);
                toBuildInstance = Instantiate(previewBuilding.prefab, snapPosition, Quaternion.identity, buildingParent);
                SetupBuildingMaterials(toBuildInstance);
                originalScale = toBuildInstance.transform.localScale;  // Save the original scale
                ShowGrid(hit.point);
            }

            toBuildInstance.transform.position = SnapPositionToGrid(hit.point);

            // Ground alignment rotation
            Quaternion groundRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            baseRotation = groundRotation;

            // Apply combined rotation with manual offset
            toBuildInstance.transform.rotation = groundRotation * Quaternion.Euler(0, rotationYOffset, 0);

            placementValid = CheckPlacement(toBuildInstance.transform.position);
            UpdateMaterialValidity(placementValid);

            AdjustScaleBasedOnTerrain(hit.normal);

            if (Input.GetMouseButtonDown(0) && placementValid)
            {
                FinishPlacement();
            }
            else if (Input.GetMouseButtonDown(0))
            {
                TriggerInvalidPlacementAnimation(toBuildInstance.transform);
                AudioManager.Instance.PlaySoundEffect(SoundEffect.ErrorClick);
            }
        }
    }

    private void ShowGrid(Vector3 center)
    {
        HideGrid();  // Clear any existing grid
        Vector3 startPosition = SnapPositionToGrid(center) - new Vector3(gridExtent * (gridSnapSize + spacing), 0, gridExtent * (gridSnapSize + spacing));

        for (int x = 0; x <= gridExtent * 2; x++)
        {
            for (int z = 0; z <= gridExtent * 2; z++)
            {
                Vector3 position = startPosition + new Vector3(x * (gridSnapSize + spacing), 0, z * (gridSnapSize + spacing));
                DrawLine(position, new Vector3(gridSnapSize, 0, 0));
                DrawLine(position, new Vector3(0, 0, gridSnapSize));
            }
        }
    }

    private void DrawLine(Vector3 start, Vector3 offset)
    {
        GameObject line = Instantiate(lineRendererPrefab, start, Quaternion.identity);
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.SetPositions(new Vector3[] { start, start + offset });
        gridLines.Add(line);
    }

    private void HideGrid()
    {
        foreach (GameObject line in gridLines)
        {
            Destroy(line);
        }
        gridLines.Clear();
    }

    private void RotateBuilding()
    {
        if (toBuildInstance != null)
        {
            rotationYOffset += 90; // Increment the Y-axis rotation offset
            if (rotationYOffset >= 360) rotationYOffset = 0; // Keep within 360 degrees
            AudioManager.Instance.PlaySoundEffect(SoundEffect.RotateBuilding);
        }
    }

    private void AdjustScaleBasedOnTerrain(Vector3 normal)
    {
        float angle = Vector3.Angle(Vector3.up, normal);
        if (angle > 2 && angle <= 25) // Threshold angle for scaling
        {
            if (!isScalingDown)
            {
                isScalingDown = true;
                Vector3 targetScale = originalScale * 0.90f;
                LeanTween.scale(toBuildInstance, targetScale, 0.3f);
            }
        }
        else if (isScalingDown)
        {
            isScalingDown = false;
            LeanTween.scale(toBuildInstance, originalScale, 0.3f);
        }
    }

    public void RegisterBuilding(BuildingData building)
    {
        activeBuildings.Add(building);
    }

    private Vector3 SnapPositionToGrid(Vector3 originalPosition)
    {
        float x = Mathf.Round(originalPosition.x / (gridSnapSize + spacing)) * (gridSnapSize + spacing);
        float z = Mathf.Round(originalPosition.z / (gridSnapSize + spacing)) * (gridSnapSize + spacing);
        return new Vector3(x, originalPosition.y, z);
    }

    private void CancelBuildingPlacement()
    {
        if (previewBuilding != null)
        {
            Debug.Log($"Destroying object: {toBuildInstance}");
            Destroy(toBuildInstance);  // Destroy the preview instance
            toBuildInstance = null;
            previewBuilding = null;
            HideGrid();
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayErrorSound();
            //Debug.Log("Building placement cancelled.");
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var renderer in originalMaterials)
        {
            renderer.Key.materials = renderer.Value;  // Restore original materials
        }
    }

    // 1 Material works on shader now, with logic inside it to trigger 2 colors (S_Placement)
    private void UpdateMaterialValidity(bool isValid)
    {
        if (placementMaterial != null)
        {
            placementMaterial.SetFloat("_IsValid", isValid ? 1f : 0f);
        }
        else
        {
            Debug.LogWarning("Placement material not set!");
        }
    }

    private void FinishPlacement()
    {
        if (placementValid && toBuildInstance != null)
        {
            // Placement is valid, so we finalize the building's setup here
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShotSFX(previewBuilding.placementSound);
            }

            toBuildInstance.transform.SetParent(buildingParent);
            RestoreOriginalMaterials();

            // Start the popping animation
            StartPopAnimation(toBuildInstance.transform);

            HideGrid();
            // to register BUILDING

            // Enable interaction with the building now that it's validly placed
            toBuildInstance.GetComponent<Building>().EnableInteraction();
            toBuildInstance = null;
            previewBuilding = null;
        }
        else
        {
            // Placement is invalid, do not allow selection or UI interaction
            Debug.Log("Attempted to place an invalid building.");
            CancelBuildingPlacement();
        }
    }

    public bool IsBuildingPresent(string buildingName)
    {
        return activeBuildings.Any(b => b.buildingName == buildingName && b.prefab.activeSelf);
    }

    private void StartPopAnimation(Transform buildingTransform)
    {
        // Set the initial scale to zero for the popping effect
        buildingTransform.localScale = Vector3.zero;

        // Animate scale up to slightly over 100% then back to 100% to create a bounce effect
        LeanTween.scale(buildingTransform.gameObject, Vector3.one * 1.1f, 0.25f).setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {
                // After popping out, scale back to the original size with a small bounce
                LeanTween.scale(buildingTransform.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeInOutQuad);
            });
    }

    private void SetupBuildingMaterials(GameObject building)
    {
        meshRenderers = building.GetComponentsInChildren<MeshRenderer>();
        originalMaterials = new Dictionary<MeshRenderer, Material[]>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            originalMaterials[renderer] = renderer.materials;  // Store original materials

            // Create an array of the same placement material for each submesh
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = placementMaterial;
            }
            renderer.materials = newMaterials;  // Apply placement material to all submeshes

            // Optionally disable collider here as well
            if (renderer.gameObject.GetComponent<Collider>() != null)
            {
                renderer.gameObject.GetComponent<Collider>().enabled = false;
            }
        }
    }


    public void SelectBuilding(BuildingData building)
    {
        previewBuilding = building; // Set by UI button
    }

    private bool CheckPlacement(Vector3 position)
    {
        float requiredDistanceFromPlayer = 10.0f;

        BoxCollider buildingCollider = toBuildInstance.GetComponent<BoxCollider>();
        if (buildingCollider == null)
        {
            Debug.LogError("No BoxCollider found on the building prefab!");
            return false;
        }

        Vector3 colliderSize = buildingCollider.size * 0.777f; 
        Quaternion colliderRotation = Quaternion.Euler(buildingCollider.transform.localEulerAngles);

        // CHK distance from player
        if (Vector3.Distance(position, player.transform.position) < requiredDistanceFromPlayer)
        {
            Debug.Log("Placement failed: Too close to player.");
            return false;
        }

        // Ground check: 
        if (!Physics.Raycast(position + Vector3.up * 5, Vector3.down, out RaycastHit groundHit, 10, groundLayer))
        {
            Debug.Log("Placement failed: Not on valid ground.");
            return false;
        }

        // Calculate ground slope angle
        float slopeAngle = Vector3.Angle(Vector3.up, groundHit.normal);
        if (slopeAngle > 27)
        {
            Debug.Log($"Placement failed: Slope too steep at {slopeAngle} degrees.");
            return false;
        }

        // Determine the 'forward' direction of the building
        Vector3 buildingForwardDirection = Vector3.forward;
        Quaternion groundNormal = Quaternion.FromToRotation(Vector3.up, groundHit.normal);
        Quaternion finalOrientation = groundNormal * Quaternion.LookRotation(buildingForwardDirection);

        // Recheck angle with the adjusted orientation
        Vector3 finalEuler = finalOrientation.eulerAngles;
        if (Mathf.Abs(finalEuler.x) > 27 || Mathf.Abs(finalEuler.y) > 27 || Mathf.Abs(finalEuler.z) > 27)
        {
            Debug.Log($"Placement failed: Building orientation exceeds tilt threshold. Angles are: x={finalEuler.x}, y={finalEuler.y}, z={finalEuler.z}");
            return false;
        }

        // Temporarily disable the collider to prevent self-detection
        buildingCollider.enabled = false;

        // chk for any building
        Collider[] colliders = Physics.OverlapBox(position, colliderSize, colliderRotation, buildingLayer);
        if (colliders.Length > 0)
        {
            buildingCollider.enabled = true; // Re-enable the collider
            Debug.Log($"Placement failed: Building intersects with {colliders.Length} other buildings.");
            return false;
        }

        Vector3 _extents = new Vector3(3, 3, 3);
        int paintMask = LayerMask.GetMask("PaintableObjects"); // PrintableObjects is the layer for all painted objects (trees, rocks, ...)

        // chk for any paintable objects
        Collider[] objectColliders = Physics.OverlapBox(position, _extents, Quaternion.identity, paintMask);
        if (objectColliders.Length > 0)
        {
            buildingCollider.enabled = true;
            Debug.Log($"Placement failed: Building intersects with {objectColliders.Length} object.");
            return false;
        }

        buildingCollider.enabled = true; // re-enable collider after the check
        Debug.Log("Placement succeeded.");
        return true;
    }



    private void TriggerInvalidPlacementAnimation(Transform buildingTransform)
    {
        Vector3 downScale = new Vector3(0.9f, 0.9f, 0.9f);

        LeanTween.scale(buildingTransform.gameObject, downScale, 0.15f).setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => {
                LeanTween.scale(buildingTransform.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutBounce);
            });
    }
}