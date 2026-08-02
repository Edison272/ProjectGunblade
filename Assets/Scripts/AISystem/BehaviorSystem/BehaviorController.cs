using System;
using System.Collections;
using System.Collections.Generic;
using GameAI.Factions;
using UnityEngine;

/// <summary>
/// A Behavior Controller keeps track of the AI for one character
/// 
/// </summary>
public class BehaviorController
{
    // Faction Information
    private Squad _factionSquad; // which squad this char is assigned to
    private FactionData _factionData => _factionSquad.Faction;
    
    // protected Dictionary<CommandMode, BehaviorModule> behavior_modules = new Dictionary<CommandMode, BehaviorModule>();

    protected Queue<Action> movement_queue = new Queue<Action>();
    protected float move_wait_time = 1;
    protected float curr_move_time = 0;
    protected float estimated_travel_time = 0; // time expected to be taken to complete a move
    protected float curr_travel_time = 0;
    protected int movement_priority = 1;
    protected float action_wait_time = 0;
    protected float curr_action_time = 0;

    // public CommandMode command;
    // public TargetType favorite_target = TargetType.Closest;
    
    [SerializeField] protected Character character;
    [field: SerializeField] public Character leader {get; private set;} // who they follow/ base their strategies around

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


#region Initializers
    public BehaviorController(Character c)
    {
        character = c;
        // anchor_position = c.GetPosition();
        // AddBehavior(CommandMode.Hold).AddBehavior(CommandMode.Follow).AddBehavior(CommandMode.Engage);
        // SetCommand(CommandMode.Hold);
    }
    public void SetLeader(Character new_leader)
    {
        leader = new_leader;
    }
    // public BehaviorController AddBehavior()
    // {
    //     switch(new_command)
    //     {
    //         case CommandMode.Hold:
    //             behavior_modules[new_command] = new HoldPositionBM(character, this);
    //             break;
    //         case CommandMode.Follow:
    //             behavior_modules[new_command] = new FollowLeaderBM(character, this);
    //             break;
    //         case CommandMode.Disperse:
    //             behavior_modules[new_command] = new HoldPositionBM(character, this);
    //             break;
    //         case CommandMode.Interact:
    //             behavior_modules[new_command] = new InteractBM(character, this);
    //             break;
    //         case CommandMode.Engage:
    //             behavior_modules[new_command] = new EngageEnemyBM(character, this);
    //             break;
    //     }
    //     return this;
    // }

    public BehaviorController SetSquad(Squad newFaction)
    {
        _factionSquad = newFaction;
        return this;
    }
#endregion
#region Update
    public virtual void UpdateAI()
    {   
        Evaluate();
        // current_module.UpdateModule();
        
        // // some tempo controllers
        // if (curr_time > 0)
        // {
        //     curr_time -= Time.fixedDeltaTime;
        // } else
        // {
        //     curr_time = 0;
        //     is_acting = !is_acting;
        //     if (!is_acting)
        //     {
        //         character.StopMainItem();
        //     }
            
        //     curr_time += is_acting ? aggro_time : rest_time;
        // }

        
        // if (character.target)
        // {
        //     // reset if target untrackable
        //     Vector2 char_to_targ_vec = character.GetPosition() - character.target.GetPosition();
        //     float vec_mag = char_to_targ_vec.magnitude;
        //     if (!character.target.IsInAction() || vec_mag > character.curr_range * 3)
        //     {
        //         character.target = null;
        //         return;
        //     }
            
        //     // only switch targets if the target is in close quarters range
        //     Character targ = GameOverseer.GetTargetCharacter(character._FactionTag, character, character.close_range, TargetType.Closest);
        //     if (targ)
        //     {
        //         character.target = targ;
        //     }

        //     // shoot target with clear Line of Sight
        //     RaycastHit2D contact = Physics2D.Linecast(character.GetPosition(), character.target.GetPosition(), GameOverseer.avoid_map_mask);
        //     if (!contact)
        //     {
        //         character.Look(character.target.GetPosition());
        //         if (vec_mag <= character.curr_range)
        //         {
        //             character.UseMainItem();
        //             Debug.DrawLine(character.GetPosition(), contact.point, Color.white);
        //         }

        //     } else
        //     {
        //         Debug.DrawLine(character.GetPosition(), character.target.GetPosition(), Color.grey);
        //     }
        // } 
        // else
        // {
        //     Character targ = GameOverseer.GetTargetCharacter(character._FactionTag, character, character.curr_range * 3, favorite_target);
        //     Debug.DrawLine(character.GetPosition(), character.GetPosition() + character.aim_dir * character.curr_range * 3, Color.black);
        //     if (targ)
        //     {
        //         character.target = targ;
        //     }
        // }

        // // orders & context will add a bunch of stuff to the queue which the AI will handle 1 by 1
        // if (character.DestinationReached)
        // {
        //     if (movement_queue.Count > 0)
        //     {
        //         Action move = movement_queue.Dequeue();
        //         move();
        //         estimated_travel_time = character.GetTravelTime();
        //         curr_travel_time = 0;
        //         prev_tile_pos = character.CurrentTilePos;
        //         //Debug.Log("movement queue in use");
        //     }

        //     estimated_travel_time = 0;
        //     curr_travel_time = 0;
        // } 
    }

    /*
    VERY IMPORTANT - allows AI to choose what they are gonna do per frame
    */
    public void Evaluate()
    {
        
    }
    #endregion
    #region Commands
    // public void SetCommand(CommandMode command)
    // {
    //     // //Debug.Log(character.name + " will " + command);
    //     // this.command = command;
    //     // current_module = behavior_modules[command];
    //     // current_module.StartModule();
    //     // estimated_travel_time = character.GetTravelTime();
    //     // curr_travel_time = 0;
    // }
    #endregion
    #region Helper Vector Weight Functions
    // protected Vector2 ObstacleAvoidanceVector(Vector2 target_dir)
    // {
    //     Vector2 net_dir = Vector2.zero;
    //     foreach(Vector2 dir in Directions2D.eight_directions)
    //     {   
    //         Vector2 normal_dir = dir.normalized;
    //         // project a detection raycast from edge of hitbox
    //         Vector2 projection_pos = character.GetPosition() + normal_dir * (character.hitbox_radius + 0.05f);
    //         Vector2 check_pos = character.GetPosition() + normal_dir * (character.hitbox_radius + avoidance_range);

    //         RaycastHit2D contact = Physics2D.Linecast(projection_pos, check_pos, GameOverseer.avoid_map_mask);
    //         float contact_weight = 1;
    //         float alignment_weight = Vector2.Dot(target_dir, (check_pos.normalized - projection_pos).normalized);;
    //         Debug.DrawLine(projection_pos, projection_pos + normal_dir, Color.gray);
    //         if (contact) // go in opposite direction if theres something in the way
    //         {
    //             contact_weight = -((contact.point - projection_pos).magnitude - avoidance_range);
    //             Debug.DrawLine(projection_pos, projection_pos + normal_dir * contact_weight * alignment_weight, Color.white);
    //         } else
    //         {
    //             Debug.DrawLine(projection_pos, projection_pos + normal_dir, Color.white);
    //         }
            
    //         net_dir += normal_dir * contact_weight * alignment_weight;

            
    //     }
    //     Debug.DrawLine(character.GetPosition(), character.GetPosition() + net_dir, Color.yellow);
    //     return net_dir;
    // }
    #endregion

    #region targetting
    // public void Character FindTarget()
    // {

    // }


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
