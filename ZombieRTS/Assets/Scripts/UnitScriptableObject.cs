using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "UnitScriptable", menuName = "ScriptableObjects/UnitScriptableObject", order = 0)]
public class UnitScriptableObject : ScriptableObject
{
    public GameObject unitPrefab;
    public string unitName;
    public int biohazardCost;
    public int bonesCost;
}
