using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public  class Biohazard_Factory : MonoBehaviour
{
    public static int totalBiohazard = 0;

    // Start is called before the first frame update
    void Awake()
    {
        StartCoroutine(generateBiohazard());
    }

    IEnumerator generateBiohazard()
    {
        yield return new WaitForSeconds(10);
        totalBiohazard += 30;

        StartCoroutine(generateBiohazard());
    }


}
