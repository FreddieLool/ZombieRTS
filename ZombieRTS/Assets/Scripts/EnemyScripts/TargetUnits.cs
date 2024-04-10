using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TargetUnits : MonoBehaviour
{
    [SerializeField] NavMeshAgent enemyAgent;
    public Transform Target;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enemyAgent.remainingDistance <= Target.position.magnitude) 
        {
            // Initiate combat between units
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClickableUnit") && Target == null)
        {
            Target = other.transform;
            enemyAgent.SetDestination(Target.position);


        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClickableUnit") && Target != null)
        {
            Target = null;
            enemyAgent.SetDestination(Target.position);

        }
    }
}
