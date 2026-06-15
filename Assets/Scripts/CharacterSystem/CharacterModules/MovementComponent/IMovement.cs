using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public interface IMovement
{
    Rigidbody2D entity_rb {get;}

    MovementComponent movement_component {get;}
    
    void Move(); // update rb position based on move_dir
    // void SetMove(Vector2 set_move_dir); // set the move_dir
    void StartMove(Vector2 move_dir);
    void StopMove(); // set move_dir to zero
    void ForceMove(Vector2 direction, float scalar, bool movement_override = false); // apply knockback or dashing
    //void ChangeSpeed(float scale_base, float duration, bool is_decaying, AbilityEffectComponent effect_controller = null); // increase or decrease speed
}

[System.Serializable]
public struct SpeedModifier
{
    // public float modifier_scale {get; private set;}
    // public float duration;
    // float decay_rate;
    // public bool effect_complete => duration <= 0;
    // private float duration_modifier;
    // private AbilityEffectComponent effect_controller;
    // public SpeedModifier(float modifier_scale, float duration, bool is_decaying, AbilityEffectComponent effect_controller)
    // {
    //     this.modifier_scale = modifier_scale;
    //     this.duration = duration;
    //     decay_rate = is_decaying ? modifier_scale/duration : 0;
    //     duration_modifier = 1;
    //     this.effect_controller = effect_controller;
    // }
    // public float UpdateModifier()
    // {
    //     if (effect_controller != null && !effect_controller.is_active)
    //     {
    //         StopEffect();
    //     }

    //     duration -= Time.deltaTime;
    //     modifier_scale -= decay_rate;
    //     return modifier_scale;
    // }
    // public void StopEffect()
    // {
    //     duration = 0;
    // }
}