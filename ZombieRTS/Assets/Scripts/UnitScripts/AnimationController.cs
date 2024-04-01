using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] UnitController script;
    void Start()
    {
        script.startedWalking.AddListener(ChangeSpeed);
    }
    public void ChangeSpeed(bool startedWalking)
    {
            animator.SetBool("isWalking", startedWalking);
    }
}
