using UnityEngine;

[CreateAssetMenu(fileName = "New Building", menuName = "Building")]
public class Building : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public int cost;
    public float buildTime;
    public float cooldown;  // Time before the building can be placed again
    public string description;
    public int capacity;
    // Defensive attributes
    public int health;
    public int armor;
    // Upgrades
    public Building upgradeTo;
    public int upgradeCost;
    public float upgradeTime;

    public AudioClip placementSound;
}
