using System.Collections.Generic;
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
            // Potentially spawn units here or reset the timer for controlled spawning
            timeSinceLastSpawn -= spawnTimer;
        }
    }

    // Spawns a unit of the given type from its corresponding pool
    public void SpawnUnit(int unitType)
    {
        if (pools == null || unitPrefabs == null)
        {
            Debug.LogError("Pools or UnitPrefabs are not initialized.");
            return;
        }

        if (unitType < 0 || unitType >= unitPrefabs.Length)
        {
            Debug.LogError($"Invalid unit type: {unitType}");
            return;
        }

        if (!pools.ContainsKey(unitType))
        {
            Debug.LogError($"No pool associated with unit type: {unitType}");
            return;
        }

        Queue<GameObject> pool = pools[unitType];
        if (pool.Count > 0)
        {
            GameObject objectToSpawn = pool.Dequeue();
            objectToSpawn.transform.position = BuildingSelectionManager.Instance.selectedBuilding.transform.position;
            objectToSpawn.SetActive(true);

            // Re-enqueue the object to enable reuse
            pool.Enqueue(objectToSpawn);
        }
        else
        {
            Debug.LogWarning($"All objects of type {unitType} are active.");
        }
    }
}
