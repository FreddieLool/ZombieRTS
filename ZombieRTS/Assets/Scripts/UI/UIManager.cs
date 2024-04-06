using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject buildingUI;
    [SerializeField] TMPro.TextMeshProUGUI BoneResource;
    [SerializeField] TMPro.TextMeshProUGUI BiohazardResource;

    private void Update()
    {
        if (buildingUI.activeSelf == false)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                buildingUI.SetActive(true);
            }
        }
        else if (buildingUI.activeSelf == true)
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                buildingUI.SetActive(false);
            }
        }
        BoneResource.text = Bone_Factory.totalBones.ToString();
        BiohazardResource.text = Biohazard_Factory.totalBiohazard.ToString();
    }
}
