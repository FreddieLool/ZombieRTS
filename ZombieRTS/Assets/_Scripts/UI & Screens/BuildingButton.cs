using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static AudioManager;

public class BuildingButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public TextMeshProUGUI boneCostText, biohazardCostText;
    public TextMeshProUGUI timeToBuildText;
    public TextMeshProUGUI cooldownText;
    //public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI otherText;
    public Button button;

    private BuildingData buildingData;
    public GameObject insufficientResourcesOverlay;



    public void Setup(BuildingData building)
    {
        buildingData = building;
        nameText.text = building.buildingName;
        iconImage.sprite = building.icon;
        boneCostText.text = GetResourceCost(building.constructionCosts, "Bone");
        biohazardCostText.text = GetResourceCost(building.constructionCosts, "Biohazard");
        timeToBuildText.text = $"{building.buildTime}s";
        cooldownText.text = $"{building.cooldown}s";
        otherText.text = $"{building.capacity}";
        //descriptionText.text = $"{building.description}";

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(ButtonClicked);

        UpdateButtonInteractivity();
    }

    private void Update()
    {
        UpdateButtonInteractivity(); 
    }

    private void UpdateButtonInteractivity()
    {
        bool hasEnoughResources = ResourceManager.Instance.HasEnoughResources(buildingData.constructionCosts);
        button.interactable = hasEnoughResources;
        insufficientResourcesOverlay.SetActive(!hasEnoughResources); 
    }

    private void ButtonClicked()
    {
        LeanTween.scale(gameObject, Vector3.one * 1.0757f, 0.1f).setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => {
                LeanTween.scale(gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeInOutQuad);
            });

        if (button.interactable)
        {
            AudioManager.Instance.PlaySoundEffect(SoundEffect.ClickOnBuilding);
        }
        else
        {
            AudioManager.Instance.PlaySoundEffect(SoundEffect.ErrorClick);
        }
    }

    private string GetResourceCost(List<ResourceCost> costs, string resourceName)
    {
        ResourceCost cost = costs.Find(c => c.resourceName == resourceName);
        return cost != null ? cost.amount.ToString() : "0";
    }
}
