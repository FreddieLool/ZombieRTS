using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Animator animator;
    private CameraEffects cameraEffects;

    void Start()
    {
        animator = GetComponent<Animator>();
        cameraEffects = GetComponent<CameraEffects>();
    }

    public void StartCameraMovement()
    {
        animator.Play("CameraPath");
    }

    public void TriggerCameraShake()
    {
        cameraEffects.StartShake();
    }
}
