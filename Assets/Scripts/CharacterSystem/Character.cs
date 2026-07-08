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
    [field: Header("Body Parts")]
    public GameObject main_body;//basically the hitbox
    public GameObject vfx_body; //the vfx body
    public Transform front;
    public Transform back;
    public Transform body;
    public Transform body_sprite;
    public Transform body_outline;
    public Transform main_hand; //always set to main hand object 
    public Transform alt_hand; //always set to off hand object
    public Transform head;
    public Transform front_particles;
    public Transform back_particles;
    public Transform true_front;
    public Transform true_back;

    [field: Header("VFX")]
    public float base_sprite_height;
    public float base_head_height;
    public Animator animator;
    protected Vector2 sprite_center; // center of mass of this srpite
    public float hitbox_radius {get; private set;}
    // character has 4 VFX states based on aim direciton, stored as two booleans, X and Y
    // T, F = Right Bottom | T, T = Right Top | F, T = Left Top | F, F = Left Bottom
    protected (bool, bool) direction_state = (true, true);
    protected (Vector2, Vector2) akimbo_hand_pos = (new Vector2 (-0.2f, 0.6f), new Vector2 (0.5f, 0.6f));  // (main pos (left), alt pos (right))
    Vector2 single_hand_pos = new Vector2 (0, 0.6f);  // (main pos, alt pos)
    // actions
    public HashedEvent MainActionStartEvent = new();
    public HashedEvent MainActionEndEvent = new();
    public HashedEvent AltActionStartEvent = new();
    public HashedEvent AltActionEndEvent = new();
    public HashedEvent ResetEvent = new();
    public HashedEvent InteractEvent = new();
    //aim & handling
    [field: Header("Aiming")]
    public Vector2 aim_dir {get; private set;} = Vector2.zero; // vector from operator to where they are looking. MAKE SURE ITS UN-NORMALIZED
    public Vector2 offset_look = Vector2.zero;
    protected Action AimStyle; // single-item or akimbo aiming?
    public  float aim_angle = 0; // angle (deg) the character is looking in
    readonly Vector2 SingleWeaponRestPosition = new Vector2(1, -1);
    
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

    [field: Header("Health Stuff")]
    [field: SerializeField] public HealthComponent health_component {get; private set;}
    public int curr_health => health_component.curr_health;
    public int max_health => base_data.health;
    public int shield => health_component.shield;
    public float health_ratio => health_component.health_ratio;
    public bool is_alive => health_component.is_alive;

    [field: Header("Health UI")]
    [SerializeField] HealthUI health_ui = new HealthUI();

    // [field: Header("Inventory")]
    // public List<Item> inventory;
    // public List<Vector2Int> item_indexes;  // Access items from the items list with indexes. Vector X for Main Item, Vector Y for Alt Item
    // protected int curr_item_index = 0;           // access items indexes list
    // public Item main_item;
    // public Item alt_item;
    // public (int, int) current_indexes {get; protected set;}
    // protected float switch_cd = 0.5f; // time the char must wait before they can switch to the next weapon
    // protected float curr_switch_cd = 0;
    // private int holding_capacity = 0;

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
    public Action<Character> OnDeath;

    #region initalizers
    void Awake()
    {
        if (base_data) {AssignBaseData(base_data);}
    }

    // get base data from a scriptable object and assign them here. Called once at when this object is created
    public void AssignBaseData(CharacterSO base_data)
    {
        this.base_data = base_data;

        // setup movement
        entity_rb = this.GetComponent<Rigidbody2D>();

        // setup VFX
        hitbox_radius = GetComponent<CircleCollider2D>().radius;
        akimbo_hand_pos = ((Vector2) main_hand.localPosition, (Vector2) alt_hand.localPosition);
        single_hand_pos = new Vector2(0, main_hand.localPosition.y);

        base_sprite_height = body_sprite.GetComponent<SpriteRenderer>().bounds.size.y;
        base_head_height = head.localPosition.y;

        // set basic sibling order of entity VFX (operator faces BOTTOM RIGHT by default)
        animator.SetBool("FaceFront", true);
        main_hand.SetSiblingIndex(4);
        front.SetSiblingIndex(3);
        vfx_body.transform.SetSiblingIndex(2);
        back.SetSiblingIndex(1);
        alt_hand.SetSiblingIndex(0);

        // setup health & related ui
        health_component = new HealthComponent(max_health, base_data.spawn_shield);
        health_ui.InitializeHealthUI(health_component);

        // setup movement
        movement_component = new MovementComponent(base_data, entity_rb);
        
        // // setup inventory
        // Item[] init_inventory = new Item[base_data.inventory.Length];
        // Vector2Int[] init_item_indexes = new Vector2Int[base_data.item_indexes.Length];
        // for (int i = 0; i < base_data.item_indexes.Length; i++)
        // {
        //     int main_item = base_data.item_indexes[i].x;
        //     int alt_item = base_data.item_indexes[i].y;
        //     init_item_indexes[i] = base_data.item_indexes[i];
        //     init_inventory[main_item] = GetItemSO(base_data.inventory[main_item]);
        //     if (alt_item > -1)
        //     {
        //         init_inventory[alt_item] = GetItemSO(base_data.inventory[alt_item], true);
        //     }
        // }
        // inventory = init_inventory.ToList<Item>();
        // item_indexes = init_item_indexes.ToList<Vector2Int>();
        // holding_capacity = Mathf.Max(item_indexes.Count, base_data.holding_capacity);

        // interactEvention_range = base_data.interactEvention_range;

        // // set initial active items
        // foreach(Item item in inventory)
        // {
        //     item.NewUser(this);
        //     item.UnequipItem();
        // }

        // // setup AI
        // CreateBehaviorController();

        GetReady();
    }
    // make sure the operator LOOKS ready
    public void GetReady()
    {        
        //EquipActive(0);
        // equip items
        // SetSwitchItem();
        SetAimStyle(true);
        // initialize default look position
        aim_dir = SingleWeaponRestPosition;
        Look(entity_rb.position + aim_dir);
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

    // Initialize op if it's a prefab that's placed on the scene, and has 
    public void Start()
    {
        if (!entity_rb && base_data)
        {
            AssignBaseData(base_data);
        }
    }

    #endregion

    #region Player Input
    public void ConnectInputs(InputEventRelay inputRelay)
    {
        inputRelay.ConnectEvent(InputEvent.Character_MoveStart, (Action<Vector2>)StartMove);
        inputRelay.ConnectEvent(InputEvent.Character_MoveEnd, StopMove);

        // inputRelay.ConnectEvent(InputEvent.Character_MoveStart, MainActionStartEvent.Invoke);
        // inputRelay.ConnectEvent(InputEvent.Character_MoveEnd, MainActionEndEvent.Invoke);
        // inputRelay.ConnectEvent(InputEvent.Character_MoveStart, AltActionStartEvent.Invoke);
        // inputRelay.ConnectEvent(InputEvent.Character_MoveEnd, AltActionEndEvent.Invoke);

        inputRelay.ConnectEvent(InputEvent.Usable_Reset, ResetEvent.Invoke);
        //inputRelay.ConnectEvent(InputEvent.Character_MoveStop, (Action)StopMove);
    }

    public void ConnectPlayer(PlayerController player_controller)
    {
        // player_controller.OnMoveStart += StartMove;
        // player_controller.OnMoveEnd += StopMove;

        // player_controller.OnMainActionStart += MainActionStartEvent.Invoke;
        // player_controller.OnMainActionEnd += MainActionEndEvent.Invoke;
        // player_controller.OnAltActionStart += AltActionStartEvent.Invoke;
        // player_controller.OnAltActionEnd += AltActionEndEvent.Invoke;

        // player_controller.OnReset += ResetEvent.Invoke;
        // player_controller.OnInteract += InteractEvent.Invoke;
    }

    public void DisconnectPlayer(PlayerController player_controller)
    {
        
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
        
        // constantly adjust aim position, since aim doesn't snap
        Aim();

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
        UpdateBodyVFX();
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

    #region Looking & Aiming
    public void Look(Vector2 look_pos) {
        // look_dir is the direction the operator is set to look at
        Vector2 look_dir = (look_pos - entity_rb.position).normalized;

        if ((look_dir.x >= 0) != (aim_dir.x >= 0))
        {
            Vector3 look_scale = new Vector3 ((int)Mathf.Sign(look_dir.x), 1, 1);
            front.localScale = look_scale;
            body.localScale = look_scale;
            back.localScale = look_scale;
            akimbo_hand_pos.Item1.x *= -1;
            akimbo_hand_pos.Item2.x *= -1;
        }


        // aim hands and body to correct direction
        aim_dir = look_pos - entity_rb.position;
        AimStyle();
    }
    public void Aim()
    {        
        // aim the items
        // main_item?.Aim(aim_dir);
        // alt_item?.Aim(aim_dir);
    }
    void SetAimStyle(bool is_akimbo) // set the position of main & alt hands for akimbo or non-akimbo weaponry whenever weapon switch
    {
        if (is_akimbo)
        {
            // set hand index
            main_hand.SetSiblingIndex(direction_state.Item1? 4 : 0);
            alt_hand.SetSiblingIndex(direction_state.Item1? 0 : 4);
            // adjust hand positions to either side of body
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;
            AimStyle = AkimboAim;
        } else {
            // set hand index
            main_hand.SetSiblingIndex(direction_state.Item2? 4 : 0);
            alt_hand.SetSiblingIndex(direction_state.Item2? 0 : 4);
            // adjust hand positions to center mass
            main_hand.localPosition = single_hand_pos;
            alt_hand.localPosition = single_hand_pos;
            AimStyle = SingleAim;
        }
    }
    void AkimboAim() // aim two weapons from two sides of body
    {
        // check if direction state has changed
        if (direction_state.Item1 != aim_dir.x > 0) // direction_state.Item1 = true -> facing right
        {
            direction_state.Item1 = aim_dir.x > 0; // update direction state
            
            // switch hand indexes
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(main_hand.GetSiblingIndex() == 4 ? 0 : 4);

            // switch hand positions
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;
        }

        if (direction_state.Item2 != aim_dir.y < 0) // direction_state.Item2 = true -> facing down (front)
        {
            direction_state.Item2 = aim_dir.y < 0; // update direction state
            
            // switch front & back index
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front.GetSiblingIndex() == 3 ? 1 : 3);

            // switch hand positions
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;

            animator.SetBool("FaceFront", direction_state.Item2); // face front
        }
    }
    void SingleAim() // aim one weapon from center of mass
    {   
        if(direction_state.Item2 != aim_dir.y <= 0) {
            direction_state.Item2 = aim_dir.y <= 0; // update direction state

            // switch hand  indexes
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(main_hand.GetSiblingIndex() == 4 ? 0 : 4);   
            // switch front/back indexes
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front.GetSiblingIndex() == 3 ? 1 : 3);

            animator.SetBool("FaceFront", direction_state.Item2); // face front
        }
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
    #region  Inventory Management
    
    // public bool HasAltAction() // returns true if the operator is currently wielding two items or an multi-state items
    // {
    //     return item_indexes[curr_item_index].y != -1;  // return true for one action, and false for two actions
    // }
    // void EquipActive(int index)  // equip the currently selected weapons (the ones the operator is currently holding)
    // {
    //     main_item = inventory[item_indexes[index].x];
    //     if (item_indexes[index].y == -1)
    //     {
    //         alt_item = null;
    //     } 
    //     else
    //     {
    //         alt_item = inventory[item_indexes[index].y];
    //     }
    //     curr_range = base_range + GetRangeScalar();
    // }
    // void UnequipActive() // unequip animation for the currently selected weapons
    // {
    //     main_item.UnequipItem();
    //     alt_item?.UnequipItem();
    // }

    // public Item GetItemSO(ItemSO new_item, bool on_alt_hand = false)
    // {
    //     Transform hand_hold = on_alt_hand ? alt_hand : main_hand;
    //     return new_item.GenerateItem(hand_hold);
    // }
    // public Item PickupItem(Item new_item)
    // {
    //     Item switch_out_item = null;
    //     if (item_indexes.Count >= holding_capacity)
    //     {            
    //         // switch out current item with the new pickup
    //         switch_out_item = inventory[current_indexes.Item1];
    //         inventory[current_indexes.Item1] = new_item;

    //         new_item.transform.parent = main_hand;
    //         new_item.transform.localPosition = Vector3.zero;
    //         float scale = new_item.transform.localScale.x;
    //         new_item.transform.localScale = new Vector3(Mathf.Abs(scale), Mathf.Abs(scale), Mathf.Abs(scale));
    //         new_item.NewUser(this);
            
    //         EquipActive(curr_item_index); // set up the new shi
    //         curr_switch_cd = switch_cd;
    //     } 
    //     else
    //     {
    //         AddItem(new_item);
    //     }
    //     return switch_out_item;
    // }
    
    // public void SwitchItem(int spec_index = -1) // cycle between item_indexes slots, or choose a select slot with spec_index
    // {
    //     if (spec_index == curr_item_index) {return;} // dont do anything if switching to active items
        
    //     if (spec_index == -1) // typical incrementation
    //     {
    //         curr_item_index += 1;
    //         if (curr_item_index > item_indexes.Count - 1)
    //         {
    //             curr_item_index = 0;
    //         }
    //     }
    //     else // specific index
    //     {
    //         curr_item_index = Mathf.Clamp(spec_index, 0, item_indexes.Count);
    //     }
    //     UnequipActive(); //unequipped item will call the "SetSwitchItem" in animator to set the new active item
    //     current_indexes = (item_indexes[curr_item_index].x, item_indexes[curr_item_index].y);
    //     EquipActive(curr_item_index); // set up the new shi
    //     curr_switch_cd = switch_cd; // set timer before equipping new weapons
    // }
    // public IInteractEventable FindInteractEventables()
    // {
    //     ContactFilter2D interactEventable_filter = new ContactFilter2D();
    //     interactEventable_filter.SetLayerMask(GameOverseer.find_interactEventable_mask);
    //     interactEventable_filter.useLayerMask = true; // Actively use the mask
    //     interactEventable_filter.useTriggers = true;
    //     Physics2D.OverlapCircle(GetPosition(), hitbox_radius + interactEvention_range, interactEventable_filter, interactEventables_in_range);
    //     IInteractEventable closest_interactEventable = null;
    //     if (interactEventables_in_range.Count > 0)
    //     {
    //         closest_interactEventable = interactEventables_in_range[0].GetComponent<IInteractEventable>();
    //     }
    //     return closest_interactEventable;
    // }
    // public int AddItem(Item new_item)
    // {
    //     inventory.Add(new_item);
    //     item_indexes.Add(new Vector2Int(inventory.Count-1, -1));

    //     new_item.NewUser(this);
    //     new_item.UnequipItem();

    //     return item_indexes.Count-1;
    // }
    // public void SetSwitchItem() // only setup the new item VFX after the old one has been put away completely
    // {
    //     main_item.EquipItem();
    //     alt_item?.EquipItem();
    //     SetAimStyle(alt_item); // adjust how the item(s) look in the player's hands
    // }
    // public void ResetEventItemData(int specific_index = -1)
    // {
    //     if (specific_index != -1)
    //     {
    //         inventory[item_indexes[specific_index].x].ResetEventData();
    //         if (item_indexes[specific_index].y != -1)
    //         {
    //             inventory[item_indexes[-1].y].ResetEventData();
    //         }
    //     } 
    //     else
    //     {
    //         foreach(Item item in inventory)
    //         {
    //             item.ResetEventData();
    //         }            
    //     }
    // }

    #endregion
    #region Item InteractEvention
    // public void UseInteractEventable(IInteractEventable interactEventable)
    // {
    //     interactEventable.InteractEvent(this);
    // }


    // public void ResetEventItems()
    // {
    //     main_item.ResetEvent();
    //     alt_item?.ResetEvent(); // resetEvent if there's an alt item
    // }
    #endregion
    // public int GetRangeScalar()
    // {
    //     return inventory[current_indexes.Item1].GetRange();
    // }

    #region VFX Body
    public virtual Transform GetBodyPart(CharacterBodyPart body_part_type) {
        Transform body_part = main_body.transform;
        switch(body_part_type)
        {
            case CharacterBodyPart.Hitbox:
                body_part = main_body.transform;
                break;
            case CharacterBodyPart.TrueFront:
                body_part = true_front;
                break;
            case CharacterBodyPart.TrueBack:
                body_part = true_back;
                break;
            case CharacterBodyPart.Front:
                body_part = front;
                break;
            case CharacterBodyPart.Back:
                body_part = back;
                break;
            case CharacterBodyPart.SpriteBody:
                body_part = body_sprite;
                break;
            case CharacterBodyPart.MainHand:
                body_part = main_hand;
                break;
            case CharacterBodyPart.AltHand:
                body_part = alt_hand;
                break;
            case CharacterBodyPart.Head:
                body_part = head;
                break;
            case CharacterBodyPart.FrontParticles:
                body_part = front_particles;
                break;
            case CharacterBodyPart.BackParticles:
                body_part = back_particles;
                break;
        }
        return body_part;
    }

    public void PlaceOnBody(CharacterBodyPart body_part_type, Transform place_object)
    {
        Transform body_part = GetBodyPart(body_part_type);
        place_object.transform.SetParent(body_part, true);
    }

    public void UpdateBodyVFX()
    {
        //body_outline.GetComponent<SpriteRenderer>().sprite = body_sprite.GetComponent<SpriteRenderer>().sprite;
        float curr_sprite_height = body_sprite.GetComponent<SpriteRenderer>().bounds.size.y;
        head.transform.localPosition = new Vector3(0, base_head_height * curr_sprite_height/base_sprite_height, 0);
    }

    public void SetOutlineAlpha(float alpha)
    {
        Color o_c = body_outline.GetComponent<SpriteRenderer>().color;
        body_outline.GetComponent<SpriteRenderer>().color = new Color(o_c.r, o_c.b, o_c.g, alpha);
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