using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitConstruction : MonoBehaviour
{
    [SerializeField] private GameObject[] unitPrefabs;
    [SerializeField][Range(0.1f, 120f)] private float spawnTimer = 1f;
    [SerializeField][Range(1, 50)] private int poolSize = 5;
    public bool isEnemy;

    private Dictionary<int, Queue<GameObject>> pools;

    private void Awake()
    {
        InitializePools();
    }

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

        if (pools[unitType].Count > 0)
        {
            GameObject objectToSpawn = pools[unitType].Dequeue();
            objectToSpawn.SetActive(true);

            // Re-add the object to the queue to enable reuse when it gets deactivated again.
            pools[unitType].Enqueue(objectToSpawn);
        }
        else
        {
            Debug.LogWarning($"All objects of type {unitType} are active.");
        }
    }
}
