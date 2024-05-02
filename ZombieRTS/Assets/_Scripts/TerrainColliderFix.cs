using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainColliderFix : MonoBehaviour
{
    private void Awake()
    {
        // not needed anymore
        // fix for terrain objects/trees not having colliders on
        GetComponent<TerrainCollider>().enabled = false;
        GetComponent<TerrainCollider>().enabled = true;
    }
}
