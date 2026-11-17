using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class MovementComponent
{
    [field: Header("Base Data")]
    [field: SerializeField] public float BaseMass {get; private set;} = 10;
    [field: SerializeField] public float BaseSpeed {get; private set;} = 3;   
    readonly float SlideTime = 0.05f;
    // speed data scalar
    [field: Header("Scaling Data")]
    [field: SerializeField] public float WeightScale {get; private set;} = 1; // how much the operator can resist external forces
    [field: SerializeField] public float SpeedScale {get; private set;} = 1; // maximum speed an operator can move at
    
    [field: Header("Runtime Data")]
    [SerializeField] private bool showRuntimeData = false;
    [field: SerializeField] public float MoveSpeed {get; private set;} = 1; // maximum speed an operator can move at
    public Vector2 MoveDir {get; private set;} = Vector2.zero;
    public Vector2 MovePos {get; private set;} = Vector2.zero;
    public Vector2 MoveSpeedDir => MoveDir * MoveSpeed;
    public bool DestinationReached {get; private set;} = false;
    public Rigidbody2D EntityRB {get; private set;}
    [field: SerializeField] public List<SpeedModifier> MoveSpeedModifiers {get; private set;} = new List<SpeedModifier>();  

    #region Constructor
    public MovementComponent(MovementComponent copied, Rigidbody2D rigidbody)
    {
        EntityRB = rigidbody;
        BaseMass = copied.BaseMass;
        BaseSpeed = copied.BaseSpeed;
        ResetMovementComponent();
    }
    public void ResetMovementComponent()
    {
        MoveSpeed = BaseSpeed;
        EntityRB.mass = BaseMass;
        EntityRB.linearDamping = 1/SlideTime;
        
        MoveDir = Vector2.zero;
        MovePos = Vector2.zero;
        DestinationReached = true;

        MoveSpeedModifiers.Clear();

        SpeedScale = 1f;
        WeightScale = 1f;
    }
    #endregion
    
    #region Set Values
    public void SetPosition(Vector2 new_position)  // completely change positions and forget where they wanted to go before
    {
        EntityRB.position = new_position;
        MovePos = new_position;
        DestinationReached = true;
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
        float targetSpeed = BaseSpeed * SpeedScale;   // desired terminal speed
        EntityRB.mass = BaseMass * WeightScale;
        EntityRB.linearDamping = 1/SlideTime; // decelerate in ~ 0.1s
        MoveSpeed = EntityRB.linearDamping * EntityRB.mass * targetSpeed;
        
        
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
        return (MovePos - GetPosition()).magnitude / BaseSpeed;
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