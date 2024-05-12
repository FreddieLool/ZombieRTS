using UnityEngine;
using System.Collections.Generic;
using System;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [SerializeField]
    private List<ResourceCost> initialResources = new List<ResourceCost>();

    private Dictionary<string, int> resources = new Dictionary<string, int>();

    public static Action OnResourcesUpdated { get; internal set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeResources();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeResources()
    {
        // Try loading saved resources
        GameSaveData savedData = SaveSystem.LoadGame();

        if (savedData != null && savedData.resources.Count > 0)
        {
            // If there are saved resources, initialize from saved data
            LoadResources(savedData.resources);
        }
        else
        {
            // Populate the resources dictionary with initial values from the inspector
            foreach (ResourceCost resource in initialResources)
            {
                resources[resource.resourceName] = resource.amount;
            }
        }
    }

    public bool HasEnoughResources(List<ResourceCost> costs)
    {
        foreach (ResourceCost cost in costs)
        {
            if (!resources.ContainsKey(cost.resourceName) || resources[cost.resourceName] < cost.amount)
                return false;
        }
        return true;
    }

    public int GetResourceAmount(string resourceName)
    {
        if (resources.TryGetValue(resourceName, out int amount))
        {
            return amount;
        }
        return 0; // Return zero if the resource is not found
    }

    public void DeductResources(List<ResourceCost> costs)
    {
        foreach (ResourceCost cost in costs)
        {
            if (resources.ContainsKey(cost.resourceName))
            {
                resources[cost.resourceName] -= cost.amount;
                UIManager.Instance.UpdateResourceUI();  // Update UI after change
            }
        }
    }

    public void AddResource(string resourceName, int amount)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName] += amount;
        }
        else
        {
            resources[resourceName] = amount;
        }

        // Trigger the resource update event
        OnResourcesUpdated?.Invoke();
    }

    public void AddResource(string resourceName, int amount, Action onResourcesUpdated = null)
    {
        if (resources.ContainsKey(resourceName))
        {
            resources[resourceName] += amount;
        }
        else
        {
            resources[resourceName] = amount;
        }

        onResourcesUpdated?.Invoke();
    }



    // load saved resources
    public void LoadResources(List<ResourceSaveData> savedResources)
    {
        resources.Clear();
        foreach (var resource in savedResources)
        {
            resources[resource.resourceName] = resource.amount;
        }
    }

    public Dictionary<string, int> GetAllResources()
    {
        return new Dictionary<string, int>(resources);
    }

    public void ResetResources()
    {
        // Reset all resource amounts to their initial values
        foreach (var resource in initialResources)
        {
            resources[resource.resourceName] = resource.amount;
        }

        // If UIManager is not initialized or available
        // action event that can be listened by UI elements interested in resource changes.
        OnResourcesUpdated?.Invoke();
    }

}
