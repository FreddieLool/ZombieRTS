using UnityEngine;
using UnityEngine.Events;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] UnitController unitController;
    void Start()
    {
        if (unitController.onMovementStateChanged == null)
        {
            unitController.onMovementStateChanged = new UnityEvent<bool>();
        }
        unitController.onMovementStateChanged.AddListener(ChangeSpeed);
    }

    public void ChangeSpeed(bool isWalking)
    {
        Debug.Log("Walking state changed to: " + isWalking);
        animator.SetBool("isWalking", isWalking);
    }


}
