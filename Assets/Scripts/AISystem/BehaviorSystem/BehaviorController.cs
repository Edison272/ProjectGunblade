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
    protected float aggro_time = 3f; // do an attack or something
    protected float rest_time = 2f; // don't attack
    protected float curr_time; // time buffer
    protected bool is_acting = true;
    // BehaviorModule current_module;

    [Header("Positioning")]
    public float base_engage_dist = 6f;
    public Vector2 anchor_position; // the general point which the operator hovers around
    public Vector2 move_to_pos; // the resulting position the bot aims to move to
    private Vector2Int prev_tile_pos;

    [Header("Evaluation")]
    public int EvaluationRadius = 20;

    // pathfinding stuff
    public readonly PathfinderModule Pathfinder;
    public Vector2Int TargetMovePos;
    public bool _pathSet = false;

    // aiming stuff
    public Vector2 TargetAimPos;
    public float AimRecovery;

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

        TargetMovePos = Vector2Int.FloorToInt(ThisCharacter.Position);        
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
        // Evaluate();
        // temporary place for target finding
        if (!TargetChar)
        {
            bool targetAllies = false;
            TargetChar = FactionSquad.FindTarget(ThisCharacter, targetAllies, TargetType.Closest);
            ThisCharacter.Aim(ThisCharacter.Position + Pathfinder.MoveDir);
        }
        // temporary attack pattern
        else
        {
            
            TargetAimPos = TargetChar.Position;
            
            ThisCharacter.Aim(TargetAimPos);
            if (curr_time <= 0)
            {
                curr_time += Time.fixedDeltaTime;
                if (curr_time >= 0)
                {
                    curr_time = aggro_time;
                    ThisCharacter.MainStart();
                }
                // reset if target if the target isn't a big threat (checks distance for now)
                // float threatScore = (TargetChar.Position - ThisCharacter.Position).magnitude / TargetChar.move_speed;
                // Debug.Log(ThisCharacter.Inventory.GetActiveSlotReadiness());
                //Debug.Log(ThisCharacter.Inventory.GetActiveSlotNeedsReset());
            }
            else
            {
                curr_time -= Time.fixedDeltaTime;
                if (curr_time <= 0)
                {
                    curr_time = -rest_time;
                    ThisCharacter.MainEnd();
                }
            }

            if (ThisCharacter.Inventory.GetActiveSlotNeedsReset())
            {
                ThisCharacter.characterRelay.Invoke(UsableEvent.ResetStart);
            }
        }


        if (!Pathfinder.IsPathing && TargetMovePos != ThisCharacter.TilePosition)
        {
            Pathfinder.FindPath(TargetMovePos);
        }

        Pathfinder.UpdatePathfinding();
    }



    /*
    VERY IMPORTANT - allows AI to choose what they are gonna do per frame
    */
    public void Evaluate()
    {
        float movementScore = -1;
        Vector2 movePos;
        float targetScore = -1;
        float utilityScore = -1;
        foreach(Vector2Int offsetVec in Directions2D.GetDirectionArray(EvaluationRadius, true))
        {
            TileProperties tileProp = MapManager.GetTileProperties(ThisCharacter.Position + offsetVec);
            if (tileProp == null)   
                continue;
            MapManager.DrawTile(Vector2Int.FloorToInt(tileProp.Position), Color.black, 0);

            // if ()
            // {
                
            // }
        }
        // if (movementScore > -1)
        // {
        //     Pathfinder.SetNewPath(movePos);
        // }
    }
    #endregion

    #region Actions
    public void SetTargetMovePos(Vector2 setPos)
    {
        Pathfinder.Reset();
        TargetMovePos = Vector2Int.FloorToInt(setPos);
    }


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
