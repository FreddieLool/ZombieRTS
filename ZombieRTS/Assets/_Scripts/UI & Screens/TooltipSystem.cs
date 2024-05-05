using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance;
    public TextMeshProUGUI tooltipText;
    public CanvasGroup tooltipCanvasGroup;
    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Initialize tooltip as invisible
        tooltipCanvasGroup.alpha = 0;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BuildingButton button = eventData.pointerEnter.GetComponent<BuildingButton>();
        if (button != null && button.buildingData != null)
        {
            string tooltipText = $"Generates {button.buildingData.resourceProductions[0].amountPerCycle} {button.buildingData.resourceProductions[0].resourceName} every {button.buildingData.resourceProductions[0].cycleTime} seconds";
            DisplayTooltip(tooltipText);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void DisplayTooltip(string text)
    {
        tooltipText.text = text;
        FadeInTooltip();
    }

    private void HideTooltip()
    {
        FadeOutTooltip();
    }

    private void FadeInTooltip()
    {
        LeanTween.alphaCanvas(tooltipCanvasGroup, 1, fadeDuration);
    }

    private void FadeOutTooltip()
    {
        LeanTween.alphaCanvas(tooltipCanvasGroup, 0, fadeDuration);
    }
}
