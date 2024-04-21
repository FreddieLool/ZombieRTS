using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyMode
{
    PathPatrol,
    BoxPatrol,
    SphereRoam
}
public class Enemy : MonoBehaviour
{
    public EnemyMode mode;
    public Transform[] patrolPoints;
    public Vector3 boxSize = Vector3.one;
    public float roamRadius = 5f;
    public Transform playerTransform;
    public float speed = 2f;
    private Vector3 nextDestination;

    private void Start()
    {
        StartCoroutine(BehaviorRoutine());
    }

    private IEnumerator BehaviorRoutine()
    {
        while (true)
        {
            switch (mode)
            {
                case EnemyMode.PathPatrol:
                    yield return StartCoroutine(PathPatrol());
                    break;
                case EnemyMode.BoxPatrol:
                    yield return StartCoroutine(BoxPatrol());
                    break;
                case EnemyMode.SphereRoam:
                    yield return StartCoroutine(SphereRoam());
                    break;
            }
        }
    }

    private IEnumerator PathPatrol()
    {
        foreach (var point in patrolPoints)
        {
            yield return MoveTo(point.position);
        }
    }

    private IEnumerator BoxPatrol()
    {
        while (true)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(-boxSize.x / 2, boxSize.x / 2),
                0, // Assuming enemy moves on a flat surface
                Random.Range(-boxSize.z / 2, boxSize.z / 2)
            ) + transform.position;
            yield return MoveTo(randomPoint);
        }
    }

    private IEnumerator SphereRoam()
    {
        while (true)
        {
            Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
            randomDirection += transform.position;
            yield return MoveTo(randomDirection);
        }
    }

    private IEnumerator MoveTo(Vector3 destination)
    {
        while (Vector3.Distance(transform.position, destination) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
            yield return null;
        }
    }
}
