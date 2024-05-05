using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI healthText;
    public TMPro.TextMeshProUGUI armorText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI upgradeCostText;

    [NonSerialized] public Building building;

    public void SetupUI(Building targetBuilding)
    {
        building = targetBuilding;
        nameText.text = building.data.buildingName;
        healthText.text = "Health: " + building.data.health.ToString();
        armorText.text = "Armor: " + building.data.armor.ToString();
        descriptionText.text = building.data.description;
        upgradeCostText.text = "Upgrade cost: " + building.data.upgradeCost.ToString();

        UpdatePosition();
    }

    void Update()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (building != null && Camera.main != null)
        {
            Vector3 offset = new Vector3(0, 17, 0);  // Adjust the Y offset as needed
            transform.position = building.transform.position + offset;
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        }
    }
}
