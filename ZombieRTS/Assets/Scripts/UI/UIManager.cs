using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject buildingUI;
    [SerializeField] TMPro.TextMeshProUGUI boneResourceText;
    [SerializeField] TMPro.TextMeshProUGUI biohazardResourceText;

    private void Update()
    {
        HandleBuildingUIToggle();
        UpdateResourceDisplays();
    }

    // Toggles the building UI visibility
    private void HandleBuildingUIToggle()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildingUI.SetActive(!buildingUI.activeSelf);
        }
    }

    // Updates the text displays for resources.
    private void UpdateResourceDisplays()
    {
        boneResourceText.text = Bone_Factory.totalBones.ToString();
        biohazardResourceText.text = Biohazard_Factory.totalBiohazard.ToString();
    }
}
