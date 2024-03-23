using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DoctorWalkingState : StateMachineBehaviour
{
    AttackController attackController;
    NavMeshAgent navMeshAgent;

    public float AttackingDistance = 1f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        attackController = animator.transform.GetComponent<AttackController>();
        navMeshAgent = animator.transform.GetComponent<NavMeshAgent>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (attackController.Target == null)
        {
            animator.SetBool("isWalking", false);
        }

        navMeshAgent.SetDestination(attackController.Target.position);
        navMeshAgent.transform.LookAt(attackController.Target.position);

        //float distanceFromTarget = Vector3.Distance(attackController.Target.position, animator.transform.position);
        //if (distanceFromTarget < AttackingDistance)
        //{
        //    animator.SetBool("isAttacking", true);
        //}
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        navMeshAgent.SetDestination(animator.transform.position);
    }

}
