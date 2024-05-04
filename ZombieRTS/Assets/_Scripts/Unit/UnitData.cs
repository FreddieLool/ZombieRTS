using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ResourceCost
{
    public string resourceName;
    public int amount;
}


[CreateAssetMenu(fileName = "New Unit Data", menuName = "Unit/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public GameObject unitPrefab;
    public Sprite icon;
    public int health;
    public int attackDamage;
    public float movementSpeed;
    public float buildTime;
    public int poolSize;  // Number of units to pre-instantiate for pooling
    public List<ResourceCost> costs = new List<ResourceCost>();
}

