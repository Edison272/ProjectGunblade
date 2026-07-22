using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using CustomDataStructures;
public enum CharacterBodyPart {None = -1, Hitbox, TrueFront, TrueBack, Front, Back, SpriteBody, MainHand, AltHand, Head, FrontParticles, BackParticles};
public class Character : MonoBehaviour, IMovement, IHealth
{
    [SerializeField] private CharacterSO base_data;
    public string character_name => base_data.character_name;

    public AnatomyComponent Anatomy;
    public Animator animator;
    [field: Header("Movement")]
    [field: SerializeField] public MovementComponent Movement {get; private set;}
    public float move_speed => Movement.move_speed; //maximum speed an operator can move at
    public Vector2 move_dir => Movement.move_dir;
    public Vector2 move_pos => Movement.move_pos;
    public bool destination_reached => Movement.destination_reached;
    public Vector2 last_move_dir => Movement.last_move_dir;
    public Vector2 force_dir => Movement.force_dir;
    public Rigidbody2D entity_rb {get; private set;}
    public Vector2Int current_tile_pos = Vector2Int.zero;
    public Vector2 Position => GetPosition(); // a more compact way of accessing player position

    [field: Header("Health Stuff")]
    [field: SerializeField] public HealthComponent health_component {get; private set;}
    public int curr_health => health_component.curr_health;
    public int max_health => base_data.health;
    public int shield => health_component.shield;
    public float health_ratio => health_component.health_ratio;
    public bool is_alive => health_component.is_alive;

    [field: Header("Health UI")]
    [SerializeField] HealthUI health_ui = new HealthUI();

    [field: Header("Inventory")]
    public InventoryComponent Inventory;

    [field: Header("InteractEventables")]
    List<Collider2D> interactEventables_in_range = new List<Collider2D>();
    public float interactEvention_range = 1;

    [field: Header("Detection")]
    // [SerializeField] CircleCollider2D range_collider;
    // ContactPoint2D[] things_in_range;
    public Character target = null;
    public float curr_range {get; private set;}
    public float base_range => base_data.range;
    public float close_range => base_data.close_range;

    // [field: Header("AI")]
    public int faction_tag = 1;
    // [SerializeField] protected bool is_AI_active = true;
    //public BehaviorController behavior_controller;    
    
    [field: Header("Event Bus")]
    public InputEventRelay characterRelay {get; private set;} // the internal relay used by the character
    private InputEventRelay controllerRelay; // reference to the input thing controlling this guy
    
    public Action<Character> OnDeath;

    #region initalizers
    // Initialize op if it's a prefab that's placed on the scene, and has 
    public void Awake()
    {
        // setup relay
        characterRelay = new InputEventRelay(
            new (Enum, Type)[] {
                (GlobalEvent.Update, typeof(Action)),

                (CharacterEvent.MoveStart, typeof(Action<Vector2>)),
                (CharacterEvent.MoveEnd, typeof(Action)),

                (CharacterEvent.MainStart, typeof(Action)), 
                (CharacterEvent.MainEnd, typeof(Action)), 
                (CharacterEvent.AltStart, typeof(Action)), 
                (CharacterEvent.AltEnd, typeof(Action)), 

                (CharacterEvent.LookPos, typeof(Action<Vector2>)), 
                (CharacterEvent.Interact, typeof(Action)), 
                (UsableEvent.ResetStart, typeof(Action)), 
            }
        );

        AssignBaseData(base_data);
        GetReady();
    }

    // get base data from a scriptable object and assign them here. Called once at when this object is created
    public void AssignBaseData(CharacterSO base_data)
    {
        this.base_data = base_data;
        // setup movement
        entity_rb = this.GetComponent<Rigidbody2D>();
        // setup VFX & Body parts
        Anatomy.Setup(entity_rb, animator);
        // setup health & related ui
        health_component = new HealthComponent(max_health, base_data.spawn_shield);
        health_ui.InitializeHealthUI(health_component);
        // setup movement
        Movement = new MovementComponent(base_data, entity_rb);
        // inventory
        Inventory = new InventoryComponent(base_data, this);
        // interactEvention_range = base_data.interactEvention_range;

        // // setup AI
        // CreateBehaviorController();

        // attach to internal function
        characterRelay.ConnectEvent(CharacterEvent.MoveStart, (Action<Vector2>)StartMove);
        characterRelay.ConnectEvent(CharacterEvent.MoveEnd, StopMove);

        characterRelay.ConnectEvent(CharacterEvent.LookPos, (Action<Vector2>)Anatomy.Look);
        characterRelay.ConnectEvent(CharacterEvent.Interact, (Action)Inventory.SwitchItemCycle);
    }
    // make sure the operator LOOKS ready
    public void GetReady()
    {        
        Inventory.SwitchItem(0);
        Anatomy.IdlePosition();
    }

    // public virtual void CreateBehaviorController() {behavior_controller = new BehaviorController(this);}
    public virtual void ResetEventData()
    {
        
    }
    // public void SetFactionTag(int tag) {faction_tag = tag;}

    public void ConnectToEventBus(Action<Character> death)
    {
        OnDeath += death;
    }



    #endregion

    #region Player Input

    // link a player or ai controller to this character
    public void LinkController(InputEventRelay inputRelay)
    {
        //unsubscribe from old user if they exist
        if (controllerRelay != null)
            characterRelay.UnlinkRelay(controllerRelay);
        
        // subscribe to old user events
        controllerRelay = inputRelay;
        if (controllerRelay != null)
            characterRelay.LinkRelay(controllerRelay);
    }

    #endregion

    #region Updates
    // Update is called once per frame
    protected virtual void Update()
    {
        // if (!IsInAction())
        // {
        //     return;
        // }
        

        // Update health
        health_component.UpdateHealth();
        Movement.UpdateMovement();
        animator.SetBool("Moving", entity_rb.linearVelocity.sqrMagnitude > 0.1f);
        animator.speed = Movement.speed_scale;

        // set switch item time duration
        Inventory.Update();

        // if (is_AI_active && !target)
        // {
        //     if (!destination_reached)
        //     {
        //         Look(move_pos);
        //     }
        //     // else
        //     // {
        //     //     Look(GetPosition() + SingleWeaponRestPosition);
        //     // }
        // }

        // update ui helpers
        //health_ui.UpdateHealthUI();

        // update vfx at the very end
        Anatomy.UpdateBodyVFX();
    }

    protected virtual void FixedUpdate()
    {
        // if (!IsInAction())
        // {
        //     return;
        // }
        // // movement
        // Move();
        // // Update AI
        // if (is_AI_active)
        // {
        //     behavior_controller.UpdateAI();
        // }
        Movement.FixedUpdateMovement();
    }

    protected virtual void LateUpdate()
    {
        if (!is_alive)
        {
            //OnDeath(this);
            Destroy(this.gameObject);
        }
    }
    #endregion

    #region Inventory

    #endregion

    #region Movement
    // completely change positions and forget where they wanted to go before
    public void SetPosition(Vector2 new_position)  {Movement.SetPosition(new_position);}
    // get directional movement, useful for dynamic & sudden maneuvers
    public void SetMove(Vector2 set_move_dir) {Movement.SetMove(set_move_dir);}
    // get target_position, useful for AI with discrete positioning
    public void SetMovePos(Vector2 set_move_pos) {Movement.SetMovePos(set_move_pos);}
    public void Move() 
    {
        // bool move_state = animator.GetBool("Moving");
        // move_state = Movement.Move(move_state);
        // animator.SetBool("Moving", move_state);
    }
    public void StartMove(Vector2 move_dir) {Movement.StartMove(move_dir);}
    public void StopMove() {Movement.StopMove();}

    // return how long it is expected to take for the operator to reach their position
    public float GetTravelTime() {return Movement.GetTravelTime();}
    public void ForceMove(Vector2 direction, float scalar, bool movement_override = false)
    {
        Movement.ForceMove(direction, scalar, movement_override);
    }
    public Vector2 GetPosition() {return entity_rb.position;}
    // public void ChangeSpeed(float scale_base, float duration, bool is_decaying, AbilityEffectComponent effect_controller = null)
    // {
    //     Movement.ChangeSpeed(scale_base, duration, is_decaying, effect_controller);
    // }
    #endregion

    #region Damage/Health System
    public virtual void ChangeHealth(int change_amt) {health_component.ChangeHealth(change_amt);}
    // public virtual void ChangeHealthTick(int change_amt, float duration, float tick_rate, AbilityEffectComponent effect_controller = null) 
    // {
    //     health_component.ChangeHealthTick(change_amt, duration, tick_rate, effect_controller);
    // }
    // public virtual void MaxHealthBoost(int boost_amt, float duration, AbilityEffectComponent effect_controller = null) {health_component.MaxHealthBoost(boost_amt, duration, effect_controller);}
    // public virtual void ShieldBoost(int boost_amt) {health_component.ShieldBoost(boost_amt);}
    #endregion

    #region  AI Stuff

    // // public ContactPoint2D[] GetAllInRange()
    // // {
    // //     range_collider.GetContacts(things_in_range);
    // //     return things_in_range;
    // // }
    // public void ToggleAI(bool is_on)
    // {
    //     is_AI_active = is_on;
    // }

    // public void SetLeader(Character new_leader)
    // {
    //     behavior_controller.SetLeader(new_leader);
    // }

    // public void SetCommandBehavior(CommandMode command)
    // {
    //     behavior_controller.SetCommand(command);
    // }
    // public virtual bool IsInAction()
    // {
    //     return is_alive;
    // }

    #endregion

    #region AI
    public TargetData GetTargetData()
    {
        return new TargetData(Position, Vector2.zero, Vector2.zero, Vector2.zero, this);
    }
    #endregion

    #region Stats & Status Changes


    #endregion

    #region Debug Gizmos
    void OnDrawGizmosSelected()
    {
        // Draw ranges when properly initialized
        if (base_data)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, curr_range);
            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(transform.position, base_range);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, close_range);
        }
    }
    #endregion
}