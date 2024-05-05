using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ResourceProduction
{
    public string resourceName;
    public int amountPerCycle;
    public float cycleTime; // Time in seconds between resource production cycles
}

[CreateAssetMenu(fileName = "New Building", menuName = "Building")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public Sprite icon;
    public List<ResourceCost> constructionCosts;  // Costs to build
    public float buildTime;
    public float cooldown;  // Time before the building can be placed again
    public string description;
    public int capacity;
    // Defensive attributes
    public int health;
    public int armor;
    // Upgrades
    public BuildingData upgradeTo;
    public int upgradeCost;
    public float upgradeTime;

    public AudioClip placementSound;

    [Header("Resource Production")]
    public List<ResourceProduction> resourceProductions;
}

