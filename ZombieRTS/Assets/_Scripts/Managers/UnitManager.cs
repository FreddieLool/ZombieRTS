using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance;
    [SerializeField] private UnitData[] unitTypes;  // Array of unit data for different types
    private Dictionary<string, Queue<GameObject>> unitPools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, UnitData> unitDataDictionary = new Dictionary<string, UnitData>();
    [SerializeField] private Transform unitParent;

    GameObject playerUnit;

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

        InitializeUnitPools();
    }


    private void InitializeUnitPools()
    {
        foreach (var data in unitTypes)
        {
            unitDataDictionary[data.unitName] = data;  // Fill the dictionary

            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < data.poolSize; i++)
            {
                GameObject newUnit = Instantiate(data.unitPrefab, transform);
                newUnit.SetActive(false);
                newUnit.GetComponent<UnitController>().unitData = data;
                pool.Enqueue(newUnit);
            }
            unitPools[data.unitName] = pool;
        }
    }


    public void SpawnUnit(string unitName, Vector3 position)
    {
        if (!CanSpawn(unitName))
        {
            Debug.Log("Not enough resources.");
            return;
        }

        if (unitDataDictionary.TryGetValue(unitName, out UnitData unitData))
        {
            GameObject unit = Instantiate(unitData.unitPrefab, position, Quaternion.identity, unitParent);
            unit.SetActive(true);
            ResourceManager.Instance.DeductResources(unitData.costs);

            UnitController unitController = unit.GetComponent<UnitController>();
            if (unitController != null)
            {
                unitController.ApplyUnitData();

                Vector3 moveDirection = Random.onUnitSphere * 5;
                moveDirection.y = 0;
                Vector3 newPosition = position + moveDirection;
                unitController.MoveToLocation(newPosition);
            }
            else
            {
                Debug.LogError("UnitController not found on spawned unit.");
            }
        }
        else
        {
            Debug.LogError("Unit data not found for: " + unitName);
        }
    }


    public bool CanSpawn(string unitName)
    {
        if (unitDataDictionary.TryGetValue(unitName, out UnitData unitData))
        {
            return ResourceManager.Instance.HasEnoughResources(unitData.costs);
        }
        return false;
    }



    // enemy units

}
