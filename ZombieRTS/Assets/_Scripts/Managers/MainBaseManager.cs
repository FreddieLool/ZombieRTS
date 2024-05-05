using UnityEngine;

public class MainBaseManager : MonoBehaviour
{
    public static MainBaseManager Instance { get; private set; }
    [SerializeField] private Transform spawnPoint;
    private int unitSpawnCount = 0;  // Counter to keep track of the number of units spawned



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
            Vector3 newPosition = GetSpawnPosition();  // Get a new position for each unit
            UnitManager.Instance.SpawnUnit(unitType, newPosition);
            unitSpawnCount++;  // Increment after spawning a unit
        }
    }

    public Vector3 GetSpawnPosition()
    {
        // Calculate offset based on the number of units already spawned
        float angle = unitSpawnCount * Mathf.PI / 4;  // Divide circle into 8 parts (45 degrees each)
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 2;  // 2 units apart
        return spawnPoint.position + offset;  // Return the assigned spawn point's position with offset
    }
}
