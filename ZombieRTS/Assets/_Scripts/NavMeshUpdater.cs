using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshUpdater : MonoBehaviour
{
    public NavMeshSurface surface;
    public float updateInterval = 5.0f; // Time in seconds between updates

    private float timer;

    void Start()
    {
        if (surface == null)
            surface = GetComponent<NavMeshSurface>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateNavMesh();
        }
    }

    void UpdateNavMesh()
    {
        surface.BuildNavMesh();
    }
}
