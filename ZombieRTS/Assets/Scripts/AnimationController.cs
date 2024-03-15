using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] UnitSelectionManager script;
    void Start()
    {
        script.speedChange.AddListener(ChangeSpeed);
    }
    public void ChangeSpeed(float value)
    {
        animator.SetFloat("Speed", value);
    }
}
