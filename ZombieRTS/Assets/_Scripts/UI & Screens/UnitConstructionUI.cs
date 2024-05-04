using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitConstructionUI : MonoBehaviour
{
    public Image unitImage;
    public Slider constructionSlider;
    public TextMeshProUGUI progressText;

    private float timeToBuild;
    private float buildStartTime;
    private UnitData unitData;

    public void Initialize(Sprite unitSprite, float buildDuration, UnitData data)
    {
        unitImage.sprite = unitSprite;
        timeToBuild = buildDuration;
        buildStartTime = Time.time;
        unitData = data;

        UpdateUI(0);
    }

    void Update()
    {
        float elapsedTime = Time.time - buildStartTime;
        float progress = elapsedTime / timeToBuild;
        UpdateUI(progress);

        if (progress >= 1)
            CompleteConstruction();
    }


    private void UpdateUI(float progress)
    {
        constructionSlider.value = progress;
        progressText.text = $"{(int)(progress * 100)}%";
    }

    private void CompleteConstruction()
    {
        if (unitData != null)
        {
            Vector3 spawnPosition = MainBaseManager.Instance.GetSpawnPosition();
            UnitManager.Instance.SpawnUnit(unitData.unitName, spawnPosition);
        }
        Destroy(gameObject);
    }
}
