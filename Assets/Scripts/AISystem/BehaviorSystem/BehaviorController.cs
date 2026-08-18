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
    private Squad _factionSquad; // which squad this char is assigned to
    private FactionData _factionData => _factionSquad != null ? _factionSquad.Faction : null;
    

    // public CommandMode command;
    // public TargetType favorite_target = TargetType.Closest;
    
    [SerializeField] protected Character _character;

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
    private float avoidance_range = 1;
    public Stack<Vector2Int> path = new Stack<Vector2Int>();

    // finding targets
    public Character TargetChar;


#region Initializers
    public BehaviorController(Character c)
    {
        _character = c;
        // anchor_position = c.GetPosition();
        // AddBehavior(CommandMode.Hold).AddBehavior(CommandMode.Follow).AddBehavior(CommandMode.Engage);
        // SetCommand(CommandMode.Hold);
    }

    public BehaviorController SetSquad(Squad newFaction)
    {
        _factionSquad = newFaction;
        
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
            TargetChar = _factionSquad.FindTarget(_character, targetAllies, TargetType.Closest);
        }
        else
        {
            _character.characterRelay.Invoke(CharacterEvent.LookPos, TargetChar.Position);
        }

        if (path.Count == 0)
        {
            MapManager.FindPath(Vector2Int.RoundToInt(_character.Position), Vector2Int.RoundToInt(_character.Position + Random.insideUnitCircle * 20), path);
            Vector2 prev = _character.Position;
            foreach(Vector2 node in path)
            {
                Debug.DrawLine(prev, node, Color.green, 10);
                prev = node;
            }
        }
        else
        {
            Vector2 moveDir = path.Peek() - _character.Position;
            if (moveDir.sqrMagnitude > 0.5f)
            {
                _character.characterRelay.Invoke(CharacterEvent.MoveStart, moveDir);
            }
            else
            {
                path.Pop();
            }
            
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

    #region Commands

    #endregion

    #region Helper Vector Weight Functions

    #endregion

    #region targetting

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
