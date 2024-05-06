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
    public float speed = 2f;
    private Vector3 nextDestination;

    public float attackRange = 10.0f;

    private NavMeshAgent agent;
    private Unit unit;
    public float detectionRadius = 10f;
    public LayerMask playerLayer;
    public Transform target;
    public int attackDamage;
    public float attackRate = 1f;
    private float attackCooldown;

    private Unit targetUnit;

    private void Start()
    {
        //StartCoroutine(BehaviorRoutine());
        agent = GetComponent<NavMeshAgent>();
        unit = GetComponent<Unit>();
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindClosestTarget();
        }
        else
        {
            agent.SetDestination(target.position);
            if (Vector3.Distance(transform.position, target.position) <= unit.attackRange)
            {
                if (attackCooldown <= 0f)
                {
                    AttackTarget();
                    attackCooldown = 1f / attackRate;
                }
            }
        }

        if (attackCooldown > 0f)
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    void FindClosestTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                target = hit.transform;
                targetUnit = target.GetComponent<Unit>();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void AttackTarget()
    {
        //Debug.Log("Attacking the player with " + unit.attackDamage + " damage.");
        targetUnit.ReceiveDamage(unit.attackDamage);
        // attack logic HERE
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
