using System;
using System.Collections;
using System.Collections.Generic;
using GameAI.Factions;
using Unity.VisualScripting;
using UnityEngine;

using Random = UnityEngine.Random;

/// <summary>
/// A Behavior Controller keeps track of the AI for one character
/// 
/// </summary>
[Serializable]
public class BehaviorController
{
    // Faction Information
    public Squad FactionSquad {get; private set;} // which squad this char is assigned to
    public FactionData FactionData => FactionSquad != null ? FactionSquad.Faction : null;
    

    // public CommandMode command;
    // public TargetType favorite_target = TargetType.Closest;
    
    [SerializeField] public readonly Character ThisCharacter;

    [Header("Actions")]
    protected float aggro_time = 1; // do an attack or something
    protected float rest_time = 0f; // don't attack
    protected float curr_time; // time buffer
    protected bool is_acting = true;
    // BehaviorModule current_module;

    [Header("Positioning")]
    public float base_engage_dist = 6f;
    public Vector2 anchor_position; // the general point which the operator hovers around
    public Vector2 move_to_pos; // the resulting position the bot aims to move to
    private Vector2Int prev_tile_pos;


    // pathfinding stuff
    public readonly PathfinderModule Pathfinder;
    public Vector2Int TargetMovePos;

    // finding targets
    public Character TargetChar;


#region Initializers
    public BehaviorController(Character c)
    {
        ThisCharacter = c;
        // anchor_position = c.GetPosition();
        // AddBehavior(CommandMode.Hold).AddBehavior(CommandMode.Follow).AddBehavior(CommandMode.Engage);
        // SetCommand(CommandMode.Hold);
        Pathfinder = new PathfinderModule(this);
        
    }

    public BehaviorController SetSquad(Squad newFaction)
    {
        FactionSquad = newFaction;
        
        return this;
    }
#endregion

#region External Events

#endregion

#region Update
    public virtual void UpdateAI()
    {   
        // temporary place for target finding
        if (!TargetChar)
        {
            bool targetAllies = false;
            TargetChar = FactionSquad.FindTarget(ThisCharacter, targetAllies, TargetType.Closest);
            Pathfinder.UpdatePathfinding(ThisCharacter.Position);
            ThisCharacter.characterRelay.Invoke(CharacterEvent.LookPos, Pathfinder.MoveDir);
        }
        else
        {
            Pathfinder.UpdatePathfinding(TargetChar.Position);
            ThisCharacter.characterRelay.Invoke(CharacterEvent.LookPos, TargetChar.Position);
        }
        Evaluate();
    }



    /*
    VERY IMPORTANT - allows AI to choose what they are gonna do per frame
    */
    public void Evaluate()
    {
        
    }
    #endregion

    #region Actions



    #endregion


    #region Data Modification Functions
    public void SetActionTime(float a_time, float r_time, bool set_acting = false)
    {
        aggro_time = a_time;
        rest_time = r_time;
        is_acting = set_acting;
    }
    #endregion
}
