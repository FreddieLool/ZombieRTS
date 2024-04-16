using System.Collections.Generic;
using UnityEngine;

public enum PlacementMode
{
    Fixed,
    Valid,
    Invalid
}

public class BuildingManager : MonoBehaviour
{
    [SerializeField] private Material validPlacementMaterial;
    [SerializeField] private Material invalidPlacementMaterial;
    [SerializeField] private MeshRenderer[] meshComponents;

    private Dictionary<MeshRenderer, List<Material>> initialMaterials = new Dictionary<MeshRenderer, List<Material>>();

    [HideInInspector] public bool hasValidPlacement;
    [HideInInspector] public bool isFixed;

    private int obstacleCount;

    // Initialize component state and materials.
    private void Awake()
    {
        hasValidPlacement = true;
        isFixed = true;
        obstacleCount = 0;
        InitializeMaterials();
    }

    // Check for collisions with non-ground objects to manage placement validity
    // todo: check for trees, and only build on ground.
    private void OnTriggerEnter(Collider other)
    {
        if (isFixed || IsGround(other.gameObject)) return;

        obstacleCount++;
        UpdatePlacementMode(PlacementMode.Invalid);
    }

    // Check for objects exiting the trigger to validate placement
    private void OnTriggerExit(Collider other)
    {
        if (isFixed || IsGround(other.gameObject)) return;

        obstacleCount--;
        if (obstacleCount == 0)
            UpdatePlacementMode(PlacementMode.Valid);
    }

#if UNITY_EDITOR
    // Ensure materials are initialized correctly in the editor
    private void OnValidate()
    {
        InitializeMaterials();
    }
#endif

    // set the placement mode and update material accordingly
    public void UpdatePlacementMode(PlacementMode mode)
    {
        isFixed = mode == PlacementMode.Fixed;
        hasValidPlacement = mode != PlacementMode.Invalid;
        ApplyMaterialBasedOnMode(mode);
    }

    // Apply materials based on the current placement mode
    public void ApplyMaterialBasedOnMode(PlacementMode mode)
    {
        foreach (MeshRenderer renderer in meshComponents)
        {
            Material[] materials = mode == PlacementMode.Fixed ? initialMaterials[renderer].ToArray() : new Material[renderer.sharedMaterials.Length];
            if (mode != PlacementMode.Fixed)
            {
                Material matToApply = mode == PlacementMode.Valid ? validPlacementMaterial : invalidPlacementMaterial;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = matToApply;
                }
            }
            renderer.sharedMaterials = materials;
        }
    }

    // Initialize or reinitialize the initial materials for all mesh components
    private void InitializeMaterials()
    {
        initialMaterials.Clear();
        foreach (MeshRenderer renderer in meshComponents)
        {
            initialMaterials[renderer] = new List<Material>(renderer.sharedMaterials);
        }
    }

    // determine if a collider is considered ground.
    private bool IsGround(GameObject obj)
    {
        return ((1 << obj.layer) & BuildingPlacer.Instance.groundLayer.value) != 0;
    }
}
