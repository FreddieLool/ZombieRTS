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
        BoneResource.text = ResourceManager.totalBones.ToString();
        BiohazardResource.text = ResourceManager.totalBiohazard.ToString();
    }
}
