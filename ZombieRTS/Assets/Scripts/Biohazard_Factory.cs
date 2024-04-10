using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public  class Biohazard_Factory : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("generateBiohazard",15,15);
    }

    private void  generateBiohazard()
    {
        ResourceManager.totalBiohazard += 30;

    }


}
