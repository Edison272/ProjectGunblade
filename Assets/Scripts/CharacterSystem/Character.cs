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
public class Character : MonoBehaviour, IMovement
{
    [SerializeField] private CharacterSO base_data;
    public string character_name => base_data.character_name;

    public AnatomyComponent Anatomy;
    public Animator animator;
    
    [field: Header("Movement")]
    [field: SerializeField] public MovementComponent movement_component {get; private set;}
    public float move_speed => movement_component.move_speed; //maximum speed an operator can move at
    public Vector2 move_dir => movement_component.move_dir;
    public Vector2 move_pos => movement_component.move_pos;
    public bool destination_reached => movement_component.destination_reached;
    public Vector2 last_move_dir => movement_component.last_move_dir;
    public Vector2 force_dir => movement_component.force_dir;
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

    private InputEventRelay controllerRelay;
    public Action<Character> OnDeath;

    #region initalizers
    // Initialize op if it's a prefab that's placed on the scene, and has 
    public void Awake()
    {
        if (!entity_rb && base_data)
        {
            AssignBaseData(base_data);
        }
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
        movement_component = new MovementComponent(base_data, entity_rb);
        
        // inventory
        Inventory = new InventoryComponent(base_data.inventory, base_data.item_indexes, base_data.holding_capacity);

        // interactEvention_range = base_data.interactEvention_range;



        // // setup AI
        // CreateBehaviorController();

        GetReady();
    }
    // make sure the operator LOOKS ready
    public void GetReady()
    {        
        // EquipActive(0);
        // // equip items
        // SetSwitchItem();
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
    public void ConnectInputs(InputEventRelay inputRelay)
    {
        controllerRelay = inputRelay;
        inputRelay.ConnectEvent(InputEvent.Character_MoveStart, (Action<Vector2>)StartMove);
        inputRelay.ConnectEvent(InputEvent.Character_MoveEnd, StopMove);

        // inputRelay.ConnectEvent(InputEvent.Character_MoveStart, MainActionStartEvent.Invoke);
        // inputRelay.ConnectEvent(InputEvent.Character_MoveEnd, MainActionEndEvent.Invoke);
        // inputRelay.ConnectEvent(InputEvent.Character_MoveStart, AltActionStartEvent.Invoke);
        // inputRelay.ConnectEvent(InputEvent.Character_MoveEnd, AltActionEndEvent.Invoke);
        inputRelay.ConnectEvent(InputEvent.Character_LookPos, (Action<Vector2>)Anatomy.Look);

        // inputRelay.ConnectEvent(InputEvent.Usable_ResetStart, ResetEvent.Invoke);
        //inputRelay.ConnectEvent(InputEvent.Character_MoveStop, (Action)StopMove);

        // set initial active items
        // foreach(Item item in inventory)
        // {
        //     item.NewUser(controllerRelay, this);
        //     item.SetEquipped(false);
        // }
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
        movement_component.UpdateMovement();
        animator.SetBool("Moving", entity_rb.linearVelocity.sqrMagnitude > 0.1f);
        animator.speed = movement_component.speed_scale;

        // // set switch item time duration
        // if (curr_switch_cd > 0)
        // {
        //     curr_switch_cd -= Time.deltaTime;
        //     if (curr_switch_cd <= 0)
        //     {
        //         SetSwitchItem();
        //     }
        // }

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
        movement_component.FixedUpdateMovement();
    }

    protected virtual void LateUpdate()
    {
        // if (!is_alive)
        // {
        //     OnDeath(this);
        //     Destroy(this.gameObject);
        // }
    }
    #endregion

    #region Movement
    // completely change positions and forget where they wanted to go before
    public void SetPosition(Vector2 new_position)  {movement_component.SetPosition(new_position);}
    // get directional movement, useful for dynamic & sudden maneuvers
    public void SetMove(Vector2 set_move_dir) {movement_component.SetMove(set_move_dir);}
    // get target_position, useful for AI with discrete positioning
    public void SetMovePos(Vector2 set_move_pos) {movement_component.SetMovePos(set_move_pos);}
    public void Move() 
    {
        // bool move_state = animator.GetBool("Moving");
        // move_state = movement_component.Move(move_state);
        // animator.SetBool("Moving", move_state);
    }
    public void StartMove(Vector2 move_dir) {movement_component.StartMove(move_dir);}
    public void StopMove() {movement_component.StopMove();}

    // return how long it is expected to take for the operator to reach their position
    public float GetTravelTime() {return movement_component.GetTravelTime();}
    public void ForceMove(Vector2 direction, float scalar, bool movement_override = false)
    {
        movement_component.ForceMove(direction, scalar, movement_override);
    }
    public Vector2 GetPosition() {return entity_rb.position;}
    // public void ChangeSpeed(float scale_base, float duration, bool is_decaying, AbilityEffectComponent effect_controller = null)
    // {
    //     movement_component.ChangeSpeed(scale_base, duration, is_decaying, effect_controller);
    // }
    #endregion

    #region Damage/Health System
    // public virtual void ChangeHealth(int change_amt) {health_component.ChangeHealth(change_amt);}
    // public virtual void ChangeHealthTick(int change_amt, float duration, float tick_rate, AbilityEffectComponent effect_controller = null) 
    // {
    //     health_component.ChangeHealthTick(change_amt, duration, tick_rate, effect_controller);
    // }
    // public virtual void MaxHealthBoost(int boost_amt, float duration, AbilityEffectComponent effect_controller = null) {health_component.MaxHealthBoost(boost_amt, duration, effect_controller);}
    // public virtual void ShieldBoost(int boost_amt) {health_component.ShieldBoost(boost_amt);}
    // #endregion

    // #region  AI Stuff

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

    // public int GetRangeScalar()
    // {
    //     return inventory[current_indexes.Item1].GetRange();
    // }

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