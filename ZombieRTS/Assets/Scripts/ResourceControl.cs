using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceControl : MonoBehaviour
{
    public static int totalBiohazard = 0;
    public static int totalBones = 0;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(totalBiohazard + " " +  totalBones);
    }
}
