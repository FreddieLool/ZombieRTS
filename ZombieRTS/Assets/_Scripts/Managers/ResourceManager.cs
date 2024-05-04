using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    [SerializeField]
    private List<ResourceCost> initialResources = new List<ResourceCost>();

    private Dictionary<string, int> resources = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeResources();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeResources()
    {
        // Populate the resources dictionary with initial values from the inspector
        foreach (ResourceCost resource in initialResources)
        {
            resources[resource.resourceName] = resource.amount;
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
            UIManager.Instance.UpdateResourceUI();  // Update UI after change
        }
        else
        {
            resources[resourceName] = amount;
        }
    }
}
