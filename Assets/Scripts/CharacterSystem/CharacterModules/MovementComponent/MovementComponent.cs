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
    public readonly float base_accel_time;
    [field: Header("Movement")]
    [field: SerializeField] public float move_speed {get; private set;} = 1; // maximum speed an operator can move at
    public Vector2 move_dir {get; private set;} = Vector2.zero;
    public Vector2 move_pos {get; private set;} = Vector2.zero;
    private Vector2 lerp_move_pos = Vector2.zero;
    [field: SerializeField] public bool destination_reached {get; private set;} = false;
    public Vector2 last_move_dir {get; private set;} = Vector2.zero;
    public Vector2 force_dir {get; private set;} = Vector2.zero;
    [SerializeField] private Rigidbody2D entity_rb;
    public float force_move_time {get; private set;}
    public Vector2Int current_tile_pos = Vector2Int.zero;

    [field: Header("Acceleration")]
    public float curr_speed {get; private set;} = 0; // how fast operator is currently moving at
    public float curr_accel_time {get; private set;} = 0;
    public float max_accel_time; // amount of time operator needs to get to top move speed

    [field: Header("Stat Changes")]
    public List<SpeedModifier> move_speed_modifiers = new List<SpeedModifier>();

    #region Constructor
    public MovementComponent(float base_move_speed, float max_accel_time, Rigidbody2D entity_rb)
    {
        this.base_move_speed = base_move_speed;
        this.move_speed = base_move_speed;
        this.base_accel_time = max_accel_time;
        this.max_accel_time = max_accel_time;
        this.entity_rb = entity_rb;
    }
    public void ResetMovementComponent()
    {
        move_speed = base_move_speed;
        max_accel_time = base_accel_time;
        curr_speed = 0;
        curr_accel_time = 0;

        move_dir = Vector2.zero;
        move_pos = Vector2.zero;
        destination_reached = true;
        force_dir = Vector2.zero;
        force_move_time = 0;

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
        curr_accel_time *= Vector2.Dot(last_move_dir, move_dir);
        move_dir = set_move_dir.normalized;
        move_pos = GetPosition() + move_dir * 1000;
    }
    public void SetMovePos(Vector2 set_move_pos) // get target_position, useful for AI with discrete positioning
    {
        last_move_dir = move_dir;
        destination_reached = false;
        curr_accel_time *= Vector2.Dot(last_move_dir, move_dir);
        move_dir = (set_move_pos - GetPosition()).normalized;
        move_pos = set_move_pos;
    }
    #endregion
    #region Update
    // Called by the controlling character or whoever. Updates speed modifiers, and physics data
    public void UpdateMovement()
    {
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

        if (move_dir.sqrMagnitude > 0)
        {
            entity_rb.AddForce(move_dir * move_speed, ForceMode2D.Force);
        }
        
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
        if (curr_accel_time < max_accel_time)
        {
            curr_accel_time += Time.fixedDeltaTime;
        }
        if (curr_accel_time > max_accel_time)
        {
            curr_accel_time = max_accel_time;
        }
        return move_speed * Mathf.Min(1, curr_accel_time/max_accel_time) * modifier;
    }
    #endregion
    #region Get Values
    private Vector2 GetPosition() {return entity_rb.position;}
    public float GetTravelTime() // return how long it is expected to take for the operator to reach their position
    {
        return (move_pos - GetPosition()).magnitude / base_move_speed + max_accel_time;
    }
    #endregion
    #region Change Stats
    public void ForceMove(Vector2 direction, float scalar, bool movement_override = false)
    {
        // if movement override, the impulse force will be ignore while the player is moving 
        force_move_time = movement_override ? 0 : 1;
        entity_rb.AddForce(direction * scalar, ForceMode2D.Impulse);
    }
    // public void ChangeSpeed(float speed_modifier, float duration, bool is_decaying, AbilityEffectComponent effect_controller)
    // {
    //     curr_accel_time *= speed_modifier;
    //     move_speed_modifiers.Add(new SpeedModifier(speed_modifier, duration, is_decaying, effect_controller));
    // }
    #endregion
}