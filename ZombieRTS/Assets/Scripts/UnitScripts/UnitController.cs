using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
public class UnitController : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] NavMeshAgent unit;
    public LayerMask ground;
    public bool isCommandedToMove;
    public UnityEvent<bool> startedWalking;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ground))
            {
                startedWalking.Invoke(true);
                isCommandedToMove = true;
                unit.SetDestination(hit.point);
            }
        }

        if (unit.hasPath == false || unit.remainingDistance <= unit.stoppingDistance)
        {
            isCommandedToMove = false;
        }
    }
}
