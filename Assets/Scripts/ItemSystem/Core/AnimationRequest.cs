using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AnimationRequest
{
    public string AnimParameterName = "Use";
    public AnimatorControllerParameterType AnimParameterType = AnimatorControllerParameterType.Trigger;
    [field: SerializeField] public float _animParamValue {get; private set;}// convert to int for int arguments, and evaluate value > 0 for T/F for bool arguments

    public float AnimSpeed = 1f;
    public void Animate(Animator animator)
    {
        animator.speed = 1/AnimSpeed;
        switch(AnimParameterType)
        {
            case AnimatorControllerParameterType.Float:
                animator.SetFloat(AnimParameterName, _animParamValue);
                break;
            case AnimatorControllerParameterType.Int:
                animator.SetInteger(AnimParameterName, (int)_animParamValue);
                break;
            case AnimatorControllerParameterType.Bool:
                animator.SetBool(AnimParameterName, _animParamValue > 0? true: false);
                break;
            case AnimatorControllerParameterType.Trigger:
                animator.SetTrigger(AnimParameterName);
                break;
        }
    }

    public void AnimateBool(Animator animator, bool boolInput)
    {
        animator.SetBool(AnimParameterName, boolInput);
    }
}