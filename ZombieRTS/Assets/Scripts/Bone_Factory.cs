using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bone_Factory : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        InvokeRepeating("generateBones", 10, 10);
    }

    void generateBones()
    {
        ResourceManager.totalBones += 10;
    }
}
