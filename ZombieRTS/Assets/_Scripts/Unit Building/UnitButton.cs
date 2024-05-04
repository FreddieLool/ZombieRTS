using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UnitButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public TextMeshProUGUI boneCostText;
    public TextMeshProUGUI fleshCostText;
    public TextMeshProUGUI biohazardCostText;
    //public TextMeshProUGUI descriptionText;
    public Button button;

    public void Setup(UnitData unit)
    {
        nameText.text = unit.unitName;
        iconImage.sprite = unit.icon;

        // Default cost texts
        boneCostText.text = "0";
        fleshCostText.text = "0";
        biohazardCostText.text = "0";

        foreach (var cost in unit.costs)
        {
            switch (cost.resourceName)
            {
                case "Bone":
                    boneCostText.text = cost.amount.ToString();
                    break;
                case "Flesh":
                    fleshCostText.text = cost.amount.ToString();
                    break;
                case "Biohazard":
                    biohazardCostText.text = cost.amount.ToString();
                    break;
            }
        }

        if (button == null)
            button = GetComponent<Button>();
    }
}
