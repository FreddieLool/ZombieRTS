using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoDisplay : MonoBehaviour
{
    public static UnitInfoDisplay Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI unitNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI attackDamageText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Slider healthSlider;

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
        unitNameText.text = "" + unit.unitName;
        healthText.text = $"Health: {unit.health}";
        attackDamageText.text = $"Attack: {unit.attackDamage}";
        speedText.text = $"Speed: {unit.movementSpeed}";
        healthSlider.value = unit.health / (float)unit.maxHealth;
    }

    public void ClearInfo()
    {
        healthText.text = "";
        attackDamageText.text = "";
        speedText.text = "";
        healthSlider.value = 0;
    }
}
