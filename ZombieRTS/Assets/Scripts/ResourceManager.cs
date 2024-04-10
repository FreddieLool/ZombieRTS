using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public static int totalBiohazard = 150;
    public static int totalBones = 150;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }



    public bool HasEnoughResources(int bonesCost, int biohazardCost)
    {
        return totalBiohazard >= biohazardCost && totalBones >= bonesCost;
    }


    public void SpendResources(int bonesCost, int biohazardCost)
    {
        if (HasEnoughResources(bonesCost, biohazardCost))
        {
            totalBiohazard -= biohazardCost;
            totalBones -= bonesCost;
        }
    }

}
