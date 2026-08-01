using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class MovementComponent
{
    [field: Header("Base Data")]
    public readonly float BaseMoveSpeed = 100;
    public readonly float BaseMass = 7;
    public readonly float BaseLinearDamp = 7; 
    
    [field: Header("Movement")]
    [field: SerializeField] public float MoveSpeed {get; private set;} = 1; // maximum speed an operator can move at
    public Vector2 MoveDir {get; private set;} = Vector2.zero;
    public Vector2 MovePos {get; private set;} = Vector2.zero;
    public bool DestinationReached {get; private set;} = false;
    public Vector2 LastMoveDir {get; private set;} = Vector2.zero;
    public Rigidbody2D EntityRB {get; private set;}
    public Vector2Int CurrentTilePos {get; private set;} = Vector2Int.zero;

    [field: Header("Stat Changes")]
    [field: SerializeField] public List<SpeedModifier> MoveSpeedModifiers {get; private set;} = new List<SpeedModifier>();

    // speed data scalar
    [field: SerializeField] public float SpeedScale {get; private set;} = 1; // maximum speed an operator can move at
    [field: SerializeField] public float WeightScale {get; private set;} = 1; // how much the operator can resist external forces

    #region Constructor
    public MovementComponent(MovementComponent copied, Rigidbody2D rigidbody)
    {
        this.EntityRB = rigidbody;
        BaseMoveSpeed = copied.BaseMoveSpeed;
        BaseMass = copied.BaseMass;
        EntityRB.mass = BaseMass;
        BaseLinearDamp = EntityRB.linearDamping;
    }
    public MovementComponent(CharacterSO character_data, Rigidbody2D entity_rb)
    {
        EntityRB = entity_rb;
        BaseMoveSpeed = character_data.speed;
        BaseMass = character_data.weight;
        BaseLinearDamp = character_data.weight;
        MoveSpeed = BaseMoveSpeed;

        EntityRB = entity_rb;
        EntityRB.mass = BaseMass;
        EntityRB.linearDamping = BaseLinearDamp;
    }
    public void ResetMovementComponent()
    {
        MoveDir = Vector2.zero;
        MovePos = Vector2.zero;
        DestinationReached = true;

        MoveSpeedModifiers.Clear();

        SpeedScale = 1f;
        WeightScale = 1f;

        EntityRB.mass = BaseMass;
        EntityRB.linearDamping = BaseLinearDamp;
    }
    #endregion
    
    #region Set Values
    public void SetPosition(Vector2 new_position)  // completely change positions and forget where they wanted to go before
    {
        EntityRB.position = new_position;
        MovePos = new_position;
        DestinationReached = true;
    }
    public void SetMove(Vector2 set_MoveDir) // get directional movement, useful for dynamic & sudden maneuvers
    {
        LastMoveDir = MoveDir;
        DestinationReached = false;
        MoveDir = set_MoveDir.normalized;
        MovePos = GetPosition() + MoveDir * 1000;
    }
    public void SetMovePos(Vector2 set_MovePos) // get targetPosition, useful for AI with discrete positioning
    {
        LastMoveDir = MoveDir;
        DestinationReached = false;
        MoveDir = (set_MovePos - GetPosition()).normalized;
        MovePos = set_MovePos;
    }
    #endregion
    #region Update
    // Called by the controlling character or whoever. Updates speed modifiers, and physics data

    public void FixedUpdateMovement()
    {
        if (MoveDir.sqrMagnitude > 0)
        {
            EntityRB.AddForce(MoveDir * MoveSpeed, ForceMode2D.Force);
        }
    }
    public void UpdateMovement()
    {
        EntityRB.mass = BaseMass * WeightScale;
        EntityRB.linearDamping = BaseLinearDamp * WeightScale;
        MoveSpeed = BaseMoveSpeed * WeightScale * WeightScale * SpeedScale;
        
        // if (MoveSpeed_modifiers.Count > 0)
        // {
        //     float net_speed_modifier = 1f;
        //     for(int i = MoveSpeed_modifiers.Count-1; i >= 0; i--)
        //     {
        //         SpeedModifier speed_mod = MoveSpeed_modifiers[i];
        //         if (speed_mod.effect_complete)
        //         {
        //             // swap n pop removal
        //             int list_end = MoveSpeed_modifiers.Count - 1;
        //             MoveSpeed_modifiers[i] = MoveSpeed_modifiers[list_end];
        //             MoveSpeed_modifiers.RemoveAt(list_end); 
        //         }
        //         else
        //         {
        //             net_speed_modifier *= speed_mod.UpdateModifier();
        //             MoveSpeed_modifiers[i] = speed_mod;
        //         }
        //     }
        //     MoveSpeed = BaseMoveSpeed * net_speed_modifier;
        // }
        
    }
    #endregion

    #region Core
    // get normalized input direction, movement component starts moving!
    public void StartMove(Vector2 MoveDir)
    {
        this.MoveDir = MoveDir;
    }

    // resets internal movement data
    public void StopMove()
    {
        this.MoveDir = Vector2.zero;
    }
    private float Accelerate(float modifier = 1f)
    {
        return 0;
    }
    #endregion
    #region Get Values
    private Vector2 GetPosition() {return EntityRB.position;}
    public float GetTravelTime() // return how long it is expected to take for the operator to reach their position
    {
        return (MovePos - GetPosition()).magnitude / BaseMoveSpeed;
    }
    #endregion
    #region Change Stats
    public void ForceMove(Vector2 direction, float scalar, bool movement_override = false)
    {
        // // if movement override ()
        // float force_mult = movement_override ? WeightScale : 0;
        EntityRB.AddForce(direction * scalar, ForceMode2D.Impulse);
    }
    // public void ChangeSpeed(float speed_modifier, float duration, bool is_decaying, AbilityEffectComponent effect_controller)
    // {
    //     curr_accel_time *= speed_modifier;
    //     MoveSpeed_modifiers.Add(new SpeedModifier(speed_modifier, duration, is_decaying, effect_controller));
    // }
    #endregion
}