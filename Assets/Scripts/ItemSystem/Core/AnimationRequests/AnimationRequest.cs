using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AnimationRequest
{
    public bool SetTrigger = true;
    [ShowIf("SetTrigger")] public string AnimParameterName = "Use";
    public string AnimationClip = "Idle";

    public float AnimSpeed = 1f;
    public void Animate(Animator animator)
    {
        animator.speed = 1/AnimSpeed;

        if (SetTrigger)
            animator.SetTrigger(AnimParameterName);
        animator.Play(AnimationClip);


        // switch(AnimParameterType)
        // {
        //     case AnimatorControllerParameterType.Float:
        //         animator.SetFloat(AnimParameterName, _animParamValue);
        //         break;
        //     case AnimatorControllerParameterType.Int:
        //         animator.SetInteger(AnimParameterName, (int)_animParamValue);
        //         break;
        //     case AnimatorControllerParameterType.Bool:
        //         animator.SetBool(AnimParameterName, _animParamValue > 0? true: false);
        //         break;
        //     case AnimatorControllerParameterType.Trigger:
        //         animator.SetTrigger(AnimParameterName);
        //         break;
        // }
    }
}