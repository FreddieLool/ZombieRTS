using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingButton : MonoBehaviour
{
    public BuildingScriptableObject buildingData;

    public void OnButtonPress()
    {
        BuildingPlacer.Instance.SetBuildingPrefab(buildingData);
    }
}
