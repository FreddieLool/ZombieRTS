using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UnitConstruction : MonoBehaviour
{
    [SerializeField] GameObject UnitPrefab;
    [SerializeField][Range(0.1f, 120f)] float spawnTimer = 1f;
    [SerializeField][Range(0, 50)] int poolSize = 5;
    GameObject[] pool;
    private void Awake()
    { 
        PopulatePool();
    }
    void PopulatePool()
    {
        pool = new GameObject[poolSize];

        for (int i = 0; i < pool.Length; i++)
        {
            pool[i] = Instantiate(UnitPrefab, this.transform.GetChild(0).transform);
            pool[i].SetActive(false);
        }
    }
    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    void EnableObjectInPool()
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].activeInHierarchy == false)
            {
                pool[i].SetActive(true);
                return;
            }
        }
    }
    IEnumerator SpawnEnemy()
    {
        while (true)
        {
            EnableObjectInPool();
            yield return new WaitForSeconds(spawnTimer);
        }
    }
}
