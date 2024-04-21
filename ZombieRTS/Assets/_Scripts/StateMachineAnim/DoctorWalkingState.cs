using UnityEngine;
using UnityEngine.AI;

public class DoctorWalkingState : StateMachineBehaviour
{
    NavMeshAgent navMeshAgent;

    public float AttackingDistance = 1f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        navMeshAgent = animator.transform.GetComponent<NavMeshAgent>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UnitController unitController = animator.GetComponent<UnitController>();
        if (unitController == null)
        {
            Debug.LogError("UnitController component not found on the GameObject");
            return;
        }

        if (unitController.isCommandedToMove)
        {
            // Continue walking
            animator.SetBool("isWalking", true);
        }
        else
        {
            // Stop walking
            animator.SetBool("isWalking", false);
            if (navMeshAgent != null && navMeshAgent.hasPath)
            {
                navMeshAgent.ResetPath();
            }
        }
    }
}
