using System.Collections.Generic;
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
    public float gridSnapSize = 5.0f;  // Grid size for snapping buildings
    public float spacing = 1.0f;  // Spacing between buildings

    [Header("Visual Feedback")]
    public Material placementMaterial; // Material based on shader that has the isValid toggle

    private GameObject toBuildInstance;
    private Camera mainCamera;
    private bool placementValid;
    private MeshRenderer[] meshRenderers;


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
            }

            Vector3 newSnapPosition = SnapPositionToGrid(hit.point);
            toBuildInstance.transform.position = newSnapPosition;
            placementValid = CheckPlacement(newSnapPosition);
            UpdateMaterialValidity(placementValid);

            if (Input.GetMouseButtonDown(0))
            {
                if (placementValid)
                {
                    FinishPlacement();
                }
                else
                {
                    // Trigger invalid placement animation
                    TriggerInvalidPlacementAnimation(toBuildInstance.transform);
                    AudioManager.Instance.PlaySoundEffect(SoundEffect.ErrorClick);
                }
            }
        }
    }

    private void RotateBuilding()
    {
        if (toBuildInstance != null)
        {
            toBuildInstance.transform.Rotate(0, 90, 0);
            // Revalidate placement? (??? to check)
            Vector3 currentPos = toBuildInstance.transform.position;
            placementValid = CheckPlacement(currentPos);
            UpdateMaterialValidity(placementValid);
            AudioManager.Instance.PlaySoundEffect(SoundEffect.RotateBuilding);
        }
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

            if(AudioManager.Instance != null)
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
        float requiredDistanceFromPlayer = 5.0f;

        BoxCollider buildingCollider = toBuildInstance.GetComponent<BoxCollider>();
        if (buildingCollider == null)
        {
            Debug.LogError("No BoxCollider found on the building prefab!");
            return false;
        }

        Vector3 colliderSize = buildingCollider.size * 0.5f; // Use half-extents
        Quaternion colliderRotation = Quaternion.Euler(buildingCollider.transform.localEulerAngles);

        // Check distance from player
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

        // Temporarily disable the collider to prevent self-detection
        buildingCollider.enabled = false;

        // Check for any building
        Collider[] colliders = Physics.OverlapBox(position, colliderSize, colliderRotation, buildingLayer);
        if (colliders.Length > 0)
        {
            buildingCollider.enabled = true; // Re-enable the collider
            Debug.Log($"Placement failed: Building intersects with {colliders.Length} other buildings.");
            return false;
        }

        Vector3 halfExtents = new Vector3(3, 3, 3);  // Half the size of the building in each dimension
        int treeMask = LayerMask.GetMask("Trees");

        // Check for any trees
        Collider[] treeColliders = Physics.OverlapBox(position, halfExtents, Quaternion.identity, treeMask);
        if (treeColliders.Length > 0)
        {
            Debug.Log($"Placement failed: Building intersects with {treeColliders.Length} trees.");
            return false;
        }

        buildingCollider.enabled = true; // Re-enable the collider after the check
        Debug.Log("Placement succeeded.");
        return true;
    }

    private void TriggerInvalidPlacementAnimation(Transform buildingTransform)
    {
        // Set the initial bounce down scale
        Vector3 downScale = new Vector3(0.9f, 0.9f, 0.9f); // Compress a lil bit more

        // Animate scale down slightly and then bounce back to original size
        LeanTween.scale(buildingTransform.gameObject, downScale, 0.15f).setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => {
                // After the initial compression, bounce back to the normal size
                LeanTween.scale(buildingTransform.gameObject, Vector3.one, 0.15f).setEase(LeanTweenType.easeOutBounce);
            });
    }
}