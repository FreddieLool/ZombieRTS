using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Bone_Factory : MonoBehaviour
{
    public static int totalBones = 0;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(generateBones());
    }

    IEnumerator generateBones()
    {
        yield return new WaitForSeconds(5);
        totalBones += 10;
        StartCoroutine(generateBones());
    }
}
