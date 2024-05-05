using System.Collections;
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
    private Dictionary<Renderer, Material[]> originalMaterials;
    public float gridSnapSize = 2.0f;  // Grid size for snapping buildings
    public float spacing = 1.0f;  // Spacing between buildings
    public List<BuildingData> activeBuildings = new List<BuildingData>();
    public bool isPlacingBuilding = false; // flag for preventing clicks when placing building

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
    bool isScalingDown = false;
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
                originalScale = toBuildInstance.transform.localScale;
                isPlacingBuilding = true; // Start placing
            }

            toBuildInstance.transform.position = SnapPositionToGrid(hit.point);
            toBuildInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0, rotationYOffset, 0);

            placementValid = CheckPlacement(toBuildInstance.transform.position);
            UpdateMaterialValidity(placementValid);

            if (Input.GetMouseButtonDown(0) && placementValid)
            {
                if (ResourceManager.Instance.HasEnoughResources(previewBuilding.constructionCosts))
                {
                    ResourceManager.Instance.DeductResources(previewBuilding.constructionCosts);
                    FinishPlacement();
                    isPlacingBuilding = false; // Placement finished
                }
                else
                {
                    Debug.Log("Not enough resources.");
                    CancelBuildingPlacement();
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                TriggerInvalidPlacementAnimation(toBuildInstance.transform);
                AudioManager.Instance.PlaySoundEffect(SoundEffect.ErrorClick);
                CancelBuildingPlacement();
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
            isPlacingBuilding = false; // Placement finished
        }
    }

    private void RestoreOriginalMaterials(GameObject building)
    {
        Renderer[] renderers = building.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (originalMaterials.ContainsKey(renderer))
            {
                renderer.materials = originalMaterials[renderer];  // Restore original materials
            }
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
            AudioManager.Instance.PlayOneShotSFX(previewBuilding.placementSound);
            toBuildInstance.transform.SetParent(buildingParent);
            RestoreOriginalMaterials(toBuildInstance);
            StartBuildingConstruction(toBuildInstance, previewBuilding.buildTime);
            //HideGrid();
            //toBuildInstance.GetComponent<Building>().DisableInteraction(); // Disable interaction during construction
            RegisterBuilding(previewBuilding);

            Building buildingScript = toBuildInstance.GetComponent<Building>();
            if (buildingScript != null)
            {
                Debug.Log("Starting Particles");
                buildingScript.ActivateParticles();  // Start particles only after actual placement
            }

            toBuildInstance = null;
            previewBuilding = null;
        }
        else
        {
            Debug.Log("Attempted to place an invalid building.");
            CancelBuildingPlacement();
        }
    }

    private void StartBuildingConstruction(GameObject building, float buildTime)
    {
        LeanTween.scale(building, Vector3.one * 1.159f, 0.25f).setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {
                LeanTween.scale(building, Vector3.zero, 0.25f).setEase(LeanTweenType.easeInBack)
                    .setOnComplete(() => {
                        // Begin the bounce scaling construction process
                        BounceScale(building, originalScale, buildTime / 5, 5);
                    });
            });
    }

    private void BounceScale(GameObject building, Vector3 targetScale, float duration, int bouncesRemaining)
    {
        if (bouncesRemaining <= 0)
        {
            building.transform.localScale = targetScale; // Set the final scale
            BuildingConstructionComplete(building); // Finalize construction
            return;
        }

        // Calculate the current bounce's target scale based on the number of bounces left
        float scaleStep = (1.0f / bouncesRemaining) * (targetScale.magnitude - building.transform.localScale.magnitude);
        Vector3 currentTargetScale = building.transform.localScale + new Vector3(scaleStep, scaleStep, scaleStep);

        // Calculate overshoot scale:
        Vector3 overshootScale = currentTargetScale * 1.05f; // Overshoot a little
        overshootScale = Vector3.Min(overshootScale, targetScale);

        // Animate to the overshoot scale and then to an undershoot scale
        LeanTween.scale(building, overshootScale, duration * 0.5f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() => {
            Vector3 undershootScale = currentTargetScale * 0.95f; // Undershoot a little
            LeanTween.scale(building, undershootScale, duration * 0.5f).setEase(LeanTweenType.easeInQuad).setOnComplete(() => {
                BounceScale(building, targetScale, duration, bouncesRemaining - 1); // Continue bouncing
            });
        });
    }

    private void BuildingConstructionComplete(GameObject building)
    {
        building.GetComponent<Building>().FinishConstruction();
        StartPopAnimation(building.transform);
        AudioManager.Instance.PlaySoundEffect(SoundEffect.BuildingComplete);
    }

    public bool IsBuildingPresent(string buildingName)
    {
        return activeBuildings.Any(b => b.buildingName == buildingName && b.prefab.activeSelf);
    }

    private void StartPopAnimation(Transform buildingTransform)
    {
        LeanTween.scale(buildingTransform.gameObject, Vector3.one * 1.234f, 0.25f).setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => LeanTween.scale(buildingTransform.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeInOutQuad));
    }

    private void SetupBuildingMaterials(GameObject building)
    {
        Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
        originalMaterials = new Dictionary<Renderer, Material[]>();

        foreach (Renderer renderer in renderers)
        {
            originalMaterials[renderer] = renderer.materials;  // Store original materials
            Material[] newMaterials = new Material[renderer.materials.Length];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = placementMaterial;
            }
            renderer.materials = newMaterials;  // Apply placement material
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
        Collider[] colliders = Physics.OverlapBox(position, new Vector3(3, 3, 3), Quaternion.identity, buildingLayer);
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