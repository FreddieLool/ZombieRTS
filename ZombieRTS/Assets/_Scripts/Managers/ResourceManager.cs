using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;
    private Dictionary<string, int> resources = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool HasEnoughResources(Dictionary<string, int> costs)
    {
        foreach (var cost in costs)
        {
            if (!resources.ContainsKey(cost.Key) || resources[cost.Key] < cost.Value)
                return false;
        }
        return true;
    }

    public void ConsumeResources(Dictionary<string, int> costs)
    {
        foreach (var cost in costs)
        {
            if (resources.ContainsKey(cost.Key))
            {
                resources[cost.Key] -= cost.Value;
            }
        }
    }

    public void AddResources(string type, int amount)
    {
        if (resources.ContainsKey(type))
        {
            resources[type] += amount;
        }
        else
        {
            resources.Add(type, amount);
        }
    }
}
