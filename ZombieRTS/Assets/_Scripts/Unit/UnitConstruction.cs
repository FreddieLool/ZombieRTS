/*using System.Collections.Generic;
using UnityEngine;

public class UnitConstruction : MonoBehaviour
{
    [SerializeField] private GameObject[] unitPrefabs;
    [SerializeField][Range(0.1f, 120f)] private float spawnTimer = 1f;
    [SerializeField][Range(1, 50)] private int poolSize = 5;
    public bool isEnemy;

    private Dictionary<int, Queue<GameObject>> pools;
    private float timeSinceLastSpawn;


    void Awake()
    {
        InitializePools();
        timeSinceLastSpawn = 0;
    }

    // Tracks time since the last spawn to control spawn rate
    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;
        ManageSpawnTimer();
    }

    // Initializes the object pools based on prefabs and pool size.
    private void InitializePools()
    {
        if (unitPrefabs == null)
        {
            Debug.LogError("UnitPrefabs array has not been set in the inspector.");
            return;
        }

        pools = new Dictionary<int, Queue<GameObject>>();

        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            var unitPrefab = unitPrefabs[i];
            if (unitPrefab == null)
            {
                Debug.LogError($"Unit prefab at index {i} is not set.");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int j = 0; j < poolSize; j++)
            {
                GameObject newObj = Instantiate(unitPrefab, transform);
                newObj.SetActive(false);
                objectPool.Enqueue(newObj);
            }

            pools.Add(i, objectPool);
        }
    }

    // Handles the spawn logic based on the timer.
    private void ManageSpawnTimer()
    {
        if (timeSinceLastSpawn >= spawnTimer)
        {
            timeSinceLastSpawn -= spawnTimer;
        }
    }

    // Spawns a unit of the given type from its corresponding pool
*//*    public void SpawnUnit(UnitData unitData)
    {
        if (!CanSpawn(unitData)) return; // Check conditions like Zombie Den existence

        GameObject unitPrefab = unitData.prefab; // Assume prefab is part of UnitData
        GameObject unitObject = Instantiate(unitPrefab, spawnLocation, Quaternion.identity);
        Unit unitComponent = unitObject.GetComponent<Unit>();
        if (unitComponent != null)
        {
            unitComponent.Initialize(unitData);
        }
    }

    bool CanSpawn(UnitData unitData)
    {
        // Example: Check for Zombie Den
        return FindObjectOfType<ZombieDen>() != null;
    }
*//*

}
*/