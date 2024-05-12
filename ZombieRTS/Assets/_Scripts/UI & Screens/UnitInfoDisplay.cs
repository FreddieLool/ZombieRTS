using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnitInfoDisplay : MonoBehaviour
{
    public static UnitInfoDisplay Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Slider healthSlider;

    [SerializeField] private GameObject statsGameObject;
    [SerializeField] private GameObject infoGameObject;
    [SerializeField] private TextMeshProUGUI selectedUnitsText;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void UpdateUnitInfo(Unit unit)
    {
        if (unit != null)
        {
            unitNameText.text = unit.unitName;
            healthText.text = $"Health: {unit.health}";
            attackDamageText.text = $"Attack: {unit.attackDamage}";
            speedText.text = $"Speed: {unit.movementSpeed}";
            healthSlider.value = unit.health / (float)unit.maxHealth;
            statsGameObject.SetActive(true);
            infoGameObject.SetActive(false);
        }
        else
        {
            ClearInfo();
        }
    }

    public void UpdateSelectedUnitsCount(int count)
    {
        if (selectedUnitsText == null)
        {
            Debug.LogError("selectedUnitsText is not initialized!");
            return; // Early return to avoid NullReferenceException
        }
        selectedUnitsText.text = $"{count}"; // Update text only if the component is valid
        if (count == 0)
        {
            ClearInfo();
        }
    }


    public void ClearInfo()
    {
        unitNameText.text = "";
        healthText.text = "";
        attackDamageText.text = "";
        speedText.text = "";
        healthSlider.value = 0;
        statsGameObject.SetActive(false);
        infoGameObject.SetActive(true);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1) // Assuming '1' is your game scene index
        {
            // Ensure all references are re-established or validated here
        }
        else
        {
            ClearInfo(); // Make sure this doesn't unintentionally interfere with non-game scenes
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}
