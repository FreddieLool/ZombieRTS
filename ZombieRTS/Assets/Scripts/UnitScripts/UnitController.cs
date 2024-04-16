using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
public class UnitController : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] NavMeshAgent unitAgent;
    public LayerMask groundLayer;
    public bool isCommandedToMove;
    public UnityEvent<bool> onMovementStateChanged;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        if (!unitAgent)
        {
            Debug.LogError("NavMeshAgent not assigned.", this);
        }
    }

    // Update is called once per frame
    // Handle input and unit movement updates
    void Update()
    {
        ProcessInput();
        UpdateMovementState();
    }

    // Check for right-clicks to command unit movement
    private void ProcessInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                MoveToLocation(hit.point);
            }
        }
    }

    // Command the unit to move to the specified location
    private void MoveToLocation(Vector3 targetPosition)
    {
        if (unitAgent)
        {
            unitAgent.SetDestination(targetPosition);
            if (onMovementStateChanged != null)
            {
                onMovementStateChanged.Invoke(true);  // Indicate movement start
            }
        }
    }

    // Update the unit's movement state and check if the unit has stopped
    private void UpdateMovementState()
    {
        if (unitAgent && !unitAgent.pathPending && unitAgent.remainingDistance <= unitAgent.stoppingDistance)
        {
            if (!unitAgent.hasPath || unitAgent.velocity.sqrMagnitude == 0f)
            {
                if (onMovementStateChanged != null)
                {
                    onMovementStateChanged.Invoke(false);  // Indicate movement stop
                }
            }
        }
    }

}
