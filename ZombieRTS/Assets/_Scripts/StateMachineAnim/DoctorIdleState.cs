using UnityEngine;

public class DoctorIdleState : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Initialization if needed
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        UnitController unitController = animator.GetComponent<UnitController>();
        if (unitController == null)
        {
            Debug.LogError("UnitController component not found on the GameObject");
            return;
        }

        // Check if the unit is commanded to move and set the walking state accordingly
        animator.SetBool("isWalking", unitController.isCommandedToMove);
    }
}
