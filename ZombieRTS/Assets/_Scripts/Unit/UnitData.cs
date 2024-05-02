using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Unit/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public GameObject prefab;
    public int health;
    public int attackDamage;
    public float movementSpeed;
    public int poolSize;
    public Dictionary<string, int> cost;
}
