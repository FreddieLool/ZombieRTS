using System;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI nameText;
    public TMPro.TextMeshProUGUI healthText;
    //public TMPro.TextMeshProUGUI armorText;
    public TMPro.TextMeshProUGUI descriptionText;
    public TMPro.TextMeshProUGUI upgradeCostText;

    [NonSerialized] public Building building;

    public Button upgradeButton, removeButton;

    private void Awake()
    {
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
        removeButton.onClick.AddListener(OnRemoveClicked);
    }

    public void SetupUI(Building targetBuilding)
    {
        building = targetBuilding;
        nameText.text = building.data.buildingName;
        healthText.text = "Health: " + building.data.health.ToString();
        //armorText.text = "Armor: " + building.data.armor.ToString();
        descriptionText.text = building.data.description;
        upgradeCostText.text = "Upgrade cost: " + building.data.upgradeCost.ToString();

        UpdatePosition();
    }

    private void OnUpgradeClicked()
    {
        // Handle upgrade logic
        Debug.Log("Upgrade button clicked for " + building.data.buildingName);
    }

    private void OnRemoveClicked()
    {
        LeanTween.scale(building.gameObject, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => {
                // Refund resources
                foreach (ResourceCost cost in building.data.constructionCosts)
                {
                    ResourceManager.Instance.AddResource(cost.resourceName, cost.amount);
                }
                Destroy(building.gameObject);
                gameObject.SetActive(false);
            });
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
