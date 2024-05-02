using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;

    [SerializeField] private UnitData[] unitTypes;  // Array of unit data for different types
    private Dictionary<string, Queue<GameObject>> unitPools = new Dictionary<string, Queue<GameObject>>();
    [SerializeField] private Transform unitParent;  // Parent for instantiated units
    private Dictionary<string, GameObject> unitPrefabs = new Dictionary<string, GameObject>();

    private Dictionary<string, UnitData> unitDataDictionary = new Dictionary<string, UnitData>();


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        InitializeUnitData();
    }

    private void InitializeUnitData()
    {
        foreach (UnitData data in unitTypes)
        {
            unitDataDictionary[data.unitName] = data;
        }
    }

    public GameObject SpawnUnit(string unitName, Vector3 position)
    {
        if (!CanSpawn(unitName)) return null;

        GameObject unitPrefab = unitDataDictionary[unitName].prefab;
        GameObject newUnit = Instantiate(unitPrefab, position, Quaternion.identity);
        newUnit.SetActive(true);
        // Optionally initialize unit with specific data
        return newUnit;
    }

    public bool CanSpawn(string unitName)
    {
        if (unitDataDictionary.TryGetValue(unitName, out UnitData unitData))
        {
            return ResourceManager.Instance.HasEnoughResources(unitData.cost);
        }
        return false;
    }

}
