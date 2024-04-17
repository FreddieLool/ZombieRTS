using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] UnitController script;
    void Start()
    {
        script.onMovementStateChanged.AddListener(ChangeSpeed);
    }
    public void ChangeSpeed(bool isWalking)
    {
        animator.SetBool("isWalking", isWalking);
    }

}
