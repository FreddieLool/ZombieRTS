using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI timeToBuildText;
    public TextMeshProUGUI cooldownText;
    //public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI otherText;
    public Button button;

    public void Setup(BuildingData building)
    {
        nameText.text = building.buildingName;
        iconImage.sprite = building.icon;
        costText.text = $"{building.cost}";
        timeToBuildText.text = $"{building.buildTime}";
        cooldownText.text = $"{building.cooldown}";
        otherText.text = $"{building.capacity}";
        //descriptionText.text = $"{building.description}";

        if (button == null)
            button = GetComponent<Button>();
    }
}
