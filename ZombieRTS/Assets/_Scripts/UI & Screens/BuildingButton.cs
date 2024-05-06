using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
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

    public BuildingData buildingData;
    public GameObject insufficientResourcesOverlay;

    // for tooltip
/*    void OnEnable()
    {
        var eventTrigger = GetComponent<EventTrigger>() ?? gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { TooltipSystem.Instance.OnPointerEnter((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { TooltipSystem.Instance.OnPointerExit((PointerEventData)data); });
        eventTrigger.triggers.Add(exitEntry);
    }*/

/*    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buildingData != null)
        {
            TooltipSystem.Instance.OnPointerEnter(eventData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance.OnPointerExit(eventData);
    }*/

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
