using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class MovementComponent
{
    [field: Header("Base Data")]
    public readonly float base_move_speed;
    public readonly float base_mass;
    public readonly float base_lin_damp;
    [field: Header("Movement")]
    [field: SerializeField] public float move_speed {get; private set;} = 1; // maximum speed an operator can move at
    public Vector2 move_dir {get; private set;} = Vector2.zero;
    public Vector2 move_pos {get; private set;} = Vector2.zero;
    private Vector2 lerp_move_pos = Vector2.zero;
    [field: SerializeField] public bool destination_reached {get; private set;} = false;
    public Vector2 last_move_dir {get; private set;} = Vector2.zero;
    public Vector2 force_dir {get; private set;} = Vector2.zero;
    [SerializeField] private Rigidbody2D entity_rb;
    public Vector2Int current_tile_pos = Vector2Int.zero;

    [field: Header("Stat Changes")]
    public List<SpeedModifier> move_speed_modifiers = new List<SpeedModifier>();

    // speed data scalar
    [field: SerializeField] public float speed_scale {get; private set;} = 1; // maximum speed an operator can move at
    [field: SerializeField] public float weight_scale {get; private set;} = 1; // how much the operator can resist external forces

    #region Constructor
    public MovementComponent(CharacterSO character_data, Rigidbody2D entity_rb)
    {
        this.entity_rb = entity_rb;
        base_move_speed = character_data.speed;
        base_mass = character_data.mass;
        move_speed = base_move_speed;
        entity_rb.mass = base_mass;
        base_lin_damp = entity_rb.linearDamping;

        // set rb stats
        entity_rb.mass = character_data.mass;
    }
    public void ResetMovementComponent()
    {
        move_speed = base_move_speed;
        move_dir = Vector2.zero;
        move_pos = Vector2.zero;
        destination_reached = true;
        force_dir = Vector2.zero;

        move_speed_modifiers.Clear();
    }
    #endregion
    
    #region Set Values
    public void SetPosition(Vector2 new_position)  // completely change positions and forget where they wanted to go before
    {
        entity_rb.position = new_position;
        move_pos = new_position;
        destination_reached = true;
    }
    public void SetMove(Vector2 set_move_dir) // get directional movement, useful for dynamic & sudden maneuvers
    {
        last_move_dir = move_dir;
        destination_reached = false;
        move_dir = set_move_dir.normalized;
        move_pos = GetPosition() + move_dir * 1000;
    }
    public void SetMovePos(Vector2 set_move_pos) // get target_position, useful for AI with discrete positioning
    {
        last_move_dir = move_dir;
        destination_reached = false;
        move_dir = (set_move_pos - GetPosition()).normalized;
        move_pos = set_move_pos;
    }
    #endregion
    #region Update
    // Called by the controlling character or whoever. Updates speed modifiers, and physics data

    public void FixedUpdateMovement()
    {
        if (move_dir.sqrMagnitude > 0)
        {
            entity_rb.AddForce(move_dir * move_speed, ForceMode2D.Force);
        }
    }
    public void UpdateMovement()
    {
        entity_rb.mass = base_mass * weight_scale;
        entity_rb.linearDamping = base_lin_damp * weight_scale;
        move_speed = base_move_speed * weight_scale * weight_scale * speed_scale;
        
        // if (move_speed_modifiers.Count > 0)
        // {
        //     float net_speed_modifier = 1f;
        //     for(int i = move_speed_modifiers.Count-1; i >= 0; i--)
        //     {
        //         SpeedModifier speed_mod = move_speed_modifiers[i];
        //         if (speed_mod.effect_complete)
        //         {
        //             // swap n pop removal
        //             int list_end = move_speed_modifiers.Count - 1;
        //             move_speed_modifiers[i] = move_speed_modifiers[list_end];
        //             move_speed_modifiers.RemoveAt(list_end); 
        //         }
        //         else
        //         {
        //             net_speed_modifier *= speed_mod.UpdateModifier();
        //             move_speed_modifiers[i] = speed_mod;
        //         }
        //     }
        //     move_speed = base_move_speed * net_speed_modifier;
        // }
        
    }
    #endregion

    #region Core
    // get normalized input direction, movement component starts moving!
    public void StartMove(Vector2 move_dir)
    {
        this.move_dir = move_dir;
    }

    // resets internal movement data
    public void StopMove()
    {
        this.move_dir = Vector2.zero;
    }
    private float Accelerate(float modifier = 1f)
    {
        return 0;
    }
    #endregion
    #region Get Values
    private Vector2 GetPosition() {return entity_rb.position;}
    public float GetTravelTime() // return how long it is expected to take for the operator to reach their position
    {
        return (move_pos - GetPosition()).magnitude / base_move_speed;
    }
    #endregion
    #region Change Stats
    public void ForceMove(Vector2 direction, float scalar, bool movement_override = false)
    {
        // // if movement override ()
        // float force_mult = movement_override ? weight_scale : 0;
        entity_rb.AddForce(direction * scalar, ForceMode2D.Impulse);
    }
    // public void ChangeSpeed(float speed_modifier, float duration, bool is_decaying, AbilityEffectComponent effect_controller)
    // {
    //     curr_accel_time *= speed_modifier;
    //     move_speed_modifiers.Add(new SpeedModifier(speed_modifier, duration, is_decaying, effect_controller));
    // }
    #endregion
}