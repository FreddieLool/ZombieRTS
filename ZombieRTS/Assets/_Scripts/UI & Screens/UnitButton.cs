using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static AudioManager;

public class UnitButton : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public Image iconImage;
    public TextMeshProUGUI boneCostText;
    public TextMeshProUGUI fleshCostText;
    public TextMeshProUGUI biohazardCostText;
    //public TextMeshProUGUI descriptionText;
    public Button button;
    private UnitData unitData;
    public GameObject insufficientResourcesOverlay;

    public void Setup(UnitData unit)
    {
        unitData = unit;

        nameText.text = unit.unitName;
        iconImage.sprite = unit.icon;
        boneCostText.text = GetResourceCost(unit.costs, "Bone");
        fleshCostText.text = GetResourceCost(unit.costs, "Flesh");
        biohazardCostText.text = GetResourceCost(unit.costs, "Biohazard");

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ButtonClicked);

        UpdateButtonInteractivity();
    }

    void Update()
    {
        UpdateButtonInteractivity(); // Continuously check resources and update button state
    }

    private void UpdateButtonInteractivity()
    {
        bool hasEnoughResources = ResourceManager.Instance.HasEnoughResources(unitData.costs);
        button.interactable = hasEnoughResources;
        insufficientResourcesOverlay.SetActive(!hasEnoughResources); // Overlay active if resources are insufficient
    }

    private void ButtonClicked()
    {
        // Start the pop-up animation
        LeanTween.scale(gameObject, Vector3.one * 1.0757f, 0.1f).setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {
                LeanTween.scale(gameObject, Vector3.one, 0.1f).setEase(LeanTweenType.easeInBack);
            });

        if (button.interactable)
        {
            // Handle unit spawning or other functionality here
            UIManager.Instance.OnUnitButtonClicked(unitData);
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
