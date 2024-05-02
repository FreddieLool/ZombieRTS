using UnityEngine;

public class MainBaseManager : MonoBehaviour
{
    public static MainBaseManager Instance;
    [SerializeField] private UnitManager unitManager;
    [SerializeField] private Transform spawnPoint;

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
    }

    public void SpawnUnit(string unitType)
    {
        if (UnitManager.Instance.CanSpawn(unitType))
        {
            UnitManager.Instance.SpawnUnit(unitType, spawnPoint.position);
        }
    }
}
