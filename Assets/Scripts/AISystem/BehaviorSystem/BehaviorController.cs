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
    public Stack<Vector2> path = new Stack<Vector2>();
    Vector2 targetTile;
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
            // Vector2 targetPos = _character.Position + Random.insideUnitCircle * 10;
            Vector2 targetPos = TargetChar ? TargetChar.Position : _character.Position + Random.insideUnitCircle * 10;


            bool pathfound = MapManager.FindPath(Vector2Int.FloorToInt(_character.Position), Vector2Int.FloorToInt(targetPos), path);
            Debug.DrawLine((Vector2)Vector2Int.FloorToInt(_character.Position), (Vector2)Vector2Int.FloorToInt(targetPos), pathfound? Color.blue : Color.red, 1);
            targetTile = path.Pop();
        }
        else
        {
            Vector2 prev = path.Peek();
            foreach(Vector2 node in path)
            {
                Debug.DrawLine(prev, node, Color.green);
                prev = node;
            }

            Vector2 moveDir = targetTile - _character.Position;

            // skip tiles the character can clearly walk to, but also won't get stuck on a wall
            RaycastHit2D hit = Physics2D.Linecast(_character.Position, path.Peek(), 1 << 6);
            bool skipTile = hit.collider == null && path.Count > 1;
            if (skipTile)
            {
                foreach (Vector2Int dirVec in Directions2D.FourDirections)
                {
                    if (MapManager.HasObstacleAt(dirVec+path.Peek()) && Vector2.Dot(dirVec, -moveDir.normalized) > 0)
                    {
                        Debug.DrawLine(path.Peek(), path.Peek() + dirVec, Color.red, 2);
                        skipTile = false;
                        break;
                    }
                }
            }


            if (skipTile || moveDir.sqrMagnitude < 0.025f)
            {
                Debug.DrawLine(_character.Position, path.Peek(), Color.black, 2);
                targetTile = path.Pop();
                if (path.Count == 0)
                    _character.characterRelay.Invoke(CharacterEvent.MoveEnd);
            }
            else
            {
                Debug.DrawLine(_character.Position, hit.point, Color.white);
                _character.characterRelay.Invoke(CharacterEvent.MoveStart, moveDir.normalized);
                if (!TargetChar)
                {
                    _character.characterRelay.Invoke(CharacterEvent.LookPos, moveDir);
                }
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
