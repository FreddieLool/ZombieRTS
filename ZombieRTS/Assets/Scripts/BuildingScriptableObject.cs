using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "BuildingScriptable", menuName = "ScriptableObjects/BuildingManagerScriptableObject", order = 0)]

public class BuildingScriptableObject : ScriptableObject
{
    public GameObject buildingPrefab;
    public string buildingName;
    public int biohazardCost;
    public int bonesCost;
}
