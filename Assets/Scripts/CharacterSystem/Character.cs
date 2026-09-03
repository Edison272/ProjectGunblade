using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

using GameAI.Factions;
using Unity.Mathematics;
public enum CharacterBodyPart {None = -1, Hitbox, TrueFront, TrueBack, Front, Back, SpriteBody, MainHand, AltHand, Head, FrontParticles, BackParticles};
public class Character : MonoBehaviour, IMovement, IHealth
{
    [SerializeField] private CharacterSO base_data;
    public string character_name => base_data.character_name;

    [field: SerializeField] public AnatomyComponent Anatomy {get; private set;}
    public Animator animator;


    [field: Header("Movement")]
    [field: SerializeField] public MovementComponent Movement {get; private set;}
    public float move_speed => Movement.MoveSpeed; //maximum speed an operator can move at
    public Vector2 MovePos => Movement.MovePos;
    public bool DestinationReached => Movement.DestinationReached;
    public Vector2 LastMoveDir => Movement.LastMoveDir;
    public Rigidbody2D entity_rb => Movement.EntityRB;
    public Vector2 Position => entity_rb.position; // a more compact way of accessing player position
    public Vector2Int TilePosition => Vector2Int.FloorToInt(Position);
    public TileProperties CurrentTile; // reference to tile properties, which includes position, but also other cool goodies

    [field: Header("Health Stuff")]
    [field: SerializeField] public HealthComponent Health {get; private set;}
    public int CurrHealth => Health.CurrHealth;
    public int MaxHealth => Health.MaxHealth;
    public int shield => Health.shield;
    public float health_ratio => Health.health_ratio;
    public bool is_alive => Health.is_alive;

    [field: Header("Health UI")]
    [SerializeField] HealthUI health_ui = new HealthUI();

    // Aiming
    public Vector2 TargetAimPos {get; private set;} = Vector2.zero;
    private Quaternion _currAimRot; // save the current quaternion rotation
    public Vector2 OffsetVec {get; private set;} = Vector2.zero;
    public Vector2 AimPosition {get; private set;} = Vector2.zero;
    public float AimStrengthScale = 1;
    public float AimStrength => base_data.AimStrength * AimStrengthScale;

    [field: Header("Inventory")]
    public InventoryComponent Inventory;

    [field: Header("InteractEventables")]
    List<Collider2D> interactEventables_in_range = new List<Collider2D>();
    public float interactEvention_range = 1;

    [field: Header("Detection")]
    // [SerializeField] CircleCollider2D range_collider;
    // ContactPoint2D[] things_in_range;
    public float curr_range {get; private set;}
    public float base_range => base_data.range;
    public float close_range => base_data.close_range;

    // [field: Header("AI")]
    public string FactionTag = "None"; // string tags. be careful
    [SerializeField] protected bool isAIActive = true;
    [SerializeField] public BehaviorController _behaviorController;   
    public FactionData CharacterFaction => _behaviorController.FactionData;
    public Squad CharacterSquad => _behaviorController.FactionSquad;

    
    [field: Header("Character Control")]
    private bool MainInputActive;
    private bool AltInputActive;
    public InputEventRelay characterRelay {get; private set;} // the internal relay used by the character
    private InputEventRelay controllerRelay; // reference to the input thing controlling this guy
    
    public Action<Character> OnDeath;
    public static readonly LayerMask find_character_mask = 1 << 6; // keep this here for now

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
                (CharacterEvent.MainUpdate, typeof(Action)), 
                (CharacterEvent.MainEnd, typeof(Action)), 
                (CharacterEvent.AltStart, typeof(Action)), 
                (CharacterEvent.AltUpdate, typeof(Action)), 
                (CharacterEvent.AltEnd, typeof(Action)), 

                (CharacterEvent.LookPos, typeof(Action<Vector2>)), 
                (CharacterEvent.Interact, typeof(Action)), 
                (UsableEvent.ResetStart, typeof(Action)), 
            }
        );
        // setup movement
        Movement = new MovementComponent(base_data, GetComponent<Rigidbody2D>());
        // setup health
        Health = new HealthComponent(base_data);
        health_ui.InitializeHealthUI(Health);
        // setup VFX & Body parts
        Anatomy.Setup(entity_rb, animator);
        // setup unventory (must be done after anatomy)
        Inventory = new InventoryComponent(base_data, this);
        // // setup AI
        _behaviorController = new BehaviorController(this);
    }
    void Start()
    {
        SetFaction();
        
        GetReady();
    }

    // make sure the operator LOOKS ready
    public void GetReady()
    {        
        Inventory.SwitchItem(0);
        Anatomy.IdlePosition();
        TargetAimPos = Position;
        AimPosition = Position;
    }
    public virtual void ResetEventData()
    {
        
    }

    // used to instantiate the character for the first time or during runtime
    public Character Clone(Vector3 position)
    {
        return Instantiate(this.gameObject, position, quaternion.identity).GetComponent<Character>();
    }

    #endregion

    #region Player Input

    // link a player or ai controller to this character
    public void LinkController(InputEventRelay inputRelay)
    {
        //unsubscribe from old user if they exist
        if (controllerRelay != null) {
            controllerRelay.DisconnectEvent(CharacterEvent.MoveStart, (Action<Vector2>)StartMove);
            controllerRelay.DisconnectEvent(CharacterEvent.MoveEnd, (Action)StopMove);

            controllerRelay.DisconnectEvent(CharacterEvent.MainStart, (Action)MainStart);
            controllerRelay.DisconnectEvent(CharacterEvent.MainEnd, (Action)MainEnd);
            controllerRelay.DisconnectEvent(CharacterEvent.AltStart, (Action)AltStart);
            controllerRelay.DisconnectEvent(CharacterEvent.AltEnd, (Action)AltEnd);

            controllerRelay.DisconnectEvent(CharacterEvent.LookPos, (Action<Vector2>)Aim);
            controllerRelay.DisconnectEvent(CharacterEvent.Interact, (Action)Interact);
            controllerRelay.DisconnectEvent(UsableEvent.ResetStart, (Action)ResetActiveSlot);
        }
        // subscribe to old user events
        controllerRelay = inputRelay;
        if (controllerRelay != null) {
            controllerRelay.ConnectEvent(CharacterEvent.MoveStart, (Action<Vector2>)StartMove);
            controllerRelay.ConnectEvent(CharacterEvent.MoveEnd, StopMove);

            controllerRelay.ConnectEvent(CharacterEvent.MainStart, MainStart);
            controllerRelay.ConnectEvent(CharacterEvent.MainEnd, MainEnd);
            controllerRelay.ConnectEvent(CharacterEvent.AltStart, AltStart);
            controllerRelay.ConnectEvent(CharacterEvent.AltEnd, AltEnd);

            controllerRelay.ConnectEvent(CharacterEvent.LookPos, (Action<Vector2>)Aim);
            controllerRelay.ConnectEvent(CharacterEvent.Interact, Interact);
            controllerRelay.ConnectEvent(UsableEvent.ResetStart, ResetActiveSlot);
        }
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
        if (MainInputActive)
            characterRelay.Invoke(CharacterEvent.MainUpdate);   
        if (AltInputActive)
            characterRelay.Invoke(CharacterEvent.AltUpdate);   

        // Update health
        Health.UpdateHealth();
        Movement.UpdateMovement();
        animator.SetBool("Moving", entity_rb.linearVelocity.sqrMagnitude > 0.1f);
        animator.speed = Movement.SpeedScale;

        // set switch item time duration
        Inventory.Update();

        // if (isAIActive && !target)
        // {
        //     if (!DestinationReached)
        //     {
        //         Look(MovePos);
        //     }
        //     // else
        //     // {
        //     //     Look(GetPosition() + SingleWeaponRestPosition);
        //     // }
        // }

        // update ui helpers
        //health_ui.UpdateHealthUI();

        // handling aim offset recovery        
        if (OffsetVec.sqrMagnitude > 0.000001)
            OffsetVec = Vector2.Lerp(OffsetVec, Vector2.zero, Time.deltaTime);
            if (OffsetVec.sqrMagnitude < 0.000001)
                OffsetVec = Vector2.zero;

        Vector2 aimDir = TargetAimPos - Position;
        Quaternion aim_rot = Quaternion.LookRotation(Vector3.forward, aimDir) * Quaternion.Euler(0, 0, 90f);
        _currAimRot = Quaternion.Lerp(_currAimRot, aim_rot, Time.deltaTime * AimStrength);
        float currAimDirMag = Mathf.Sqrt(Mathf.Lerp((AimPosition - Position).sqrMagnitude, aimDir.sqrMagnitude, Time.deltaTime * AimStrength));

        Vector2 currAimDir = _currAimRot * Vector2.right * currAimDirMag;
        AimPosition = Position + currAimDir;

        Anatomy.Look(aimDir);
        characterRelay.Invoke(CharacterEvent.LookPos, AimPosition + OffsetVec * currAimDirMag);

        Debug.DrawLine(Position, TargetAimPos, Color.black);
        Debug.DrawLine(Position, AimPosition + OffsetVec * currAimDirMag, Color.gray);

        // update vfx at the very end
        Anatomy.UpdateBodyVFX();
    }

    protected virtual void FixedUpdate()
    {
        // if (!IsInAction())
        // {
        //     return;
        // }


        // // Update AI
        if (isAIActive)
        {
            _behaviorController.UpdateAI();
        }

        Movement.FixedUpdateMovement();

        // update tile occupation on global map
        CurrentTile = MapManager.UpdateCharTilePos(CurrentTile, Position);

        if (!is_alive)
        {
            //OnDeath(this);
            isAIActive = false;
            _behaviorController.SetSquad(null);
            Destroy(this.gameObject);
        }
    }

    protected virtual void LateUpdate()
    {

    }
    #endregion

    #region Aiming
    public void Aim(Vector2 lookPos)
    {
        TargetAimPos = lookPos;
    }
    public void StaggerAim(Vector2 normalDir, float scalar) // used to apply recoil, or offsets to the character's aim
    {
        OffsetVec += normalDir * scalar;
        // float magnitude = OffsetVec.magnitude;
        // if (magnitude > MaxOffsetMagnitude)
        // {
        //     OffsetVec = OffsetVec.normalized * MaxOffsetMagnitude;
        // }
    }
    #endregion

    #region  Inputs
    public void MainStart() {
        characterRelay.Invoke(CharacterEvent.MainStart);  
        MainInputActive = true;
        }
    public void MainEnd() {
        characterRelay.Invoke(CharacterEvent.MainEnd);  
        MainInputActive = false;
        }
    public void AltStart() {
        characterRelay.Invoke(CharacterEvent.AltStart);  
        AltInputActive = true;
        }
    public void AltEnd() {
        characterRelay.Invoke(CharacterEvent.AltEnd);  
        AltInputActive = false;
        }
    #endregion

    #region Inventory
    // THE DEFINITIVE INTERACITON FUNCTION
    public void ResetActiveSlot()
    {
        characterRelay.Invoke(UsableEvent.ResetStart);   
    }
    public void Interact()
    {
        characterRelay.Invoke(CharacterEvent.Interact);   
        IInteractable nearbyInteractable = FindInteractables();
        if (nearbyInteractable != null)
        {
            nearbyInteractable.Interact(this);
        }
        else
        {
            Inventory.SwitchItem(-1); 
        }
    }

    // returns false of the item could not be added to the inventory
    public Item PickupItem(Item newItem, bool setAsAlt = false) {return Inventory.PickupItem(newItem, setAsAlt);}

    public IInteractable FindInteractables()
    {
        ContactFilter2D interactEventable_filter = new ContactFilter2D();
        interactEventable_filter.SetLayerMask(IInteractable.find_interactable_mask);
        interactEventable_filter.useLayerMask = true; // Actively use the mask
        interactEventable_filter.useTriggers = true;
        Physics2D.OverlapCircle(GetPosition(), interactEvention_range, interactEventable_filter, interactEventables_in_range);
        IInteractable closest_interactEventable = null;
        if (interactEventables_in_range.Count > 0)
        {
            Debug.Log("target acquried");
            closest_interactEventable = interactEventables_in_range[0].GetComponent<IInteractable>();
        }
        return closest_interactEventable;
    }
    #endregion

    #region Movement
    // completely change positions and forget where they wanted to go before
    public void SetPosition(Vector2 new_position)  {Movement.SetPosition(new_position);}
    public void StartMove(Vector2 MoveDir) {
        characterRelay.Invoke(CharacterEvent.MoveStart);
        Movement.StartMove(MoveDir);
    }
    public void StopMove() {
        characterRelay.Invoke(CharacterEvent.MoveEnd);   
        Movement.StopMove();
    }

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
    public virtual void ChangeHealth(int change_amt) {
        Health.ChangeHealth(change_amt);
        if (change_amt > 0)
        {
            StaggerAim(Random.insideUnitCircle, 0.1f);
        }
    }
    // public virtual void ChangeHealthTick(int change_amt, float duration, float tick_rate, AbilityEffectComponent effect_controller = null) 
    // {
    //     Health.ChangeHealthTick(change_amt, duration, tick_rate, effect_controller);
    // }
    // public virtual void MaxHealthBoost(int boost_amt, float duration, AbilityEffectComponent effect_controller = null) {Health.MaxHealthBoost(boost_amt, duration, effect_controller);}
    // public virtual void ShieldBoost(int boost_amt) {Health.ShieldBoost(boost_amt);}
    #endregion

    #region  AI Stuff
    public void SetFaction()
    {
        // will join or create their own factions automatically if one is listed.
        FactionData faction = FactionManager.Instance.RegisterFaction(FactionTag);
        Squad factionSquad = faction.AddMember(this);
        _behaviorController.SetSquad(factionSquad);
        Debug.Log($"{faction.FactionName}, {factionSquad.Members.Count}");
    }

    // used when setting the affiliation of a chacter
    public void SetFactionTag(string newTag)
    {
        FactionTag = newTag;
    }
    #endregion

    #region AI
    public void SetFaction(string newTag) {FactionTag = newTag;}

    public TargetData GetTargetData(TargetDataRequest targetDataRequest)
    {
        TargetData newTargData = new TargetData(Position, Vector2.zero, Vector2.zero, Vector2.zero).SetOwner(this);

        if (targetDataRequest.HomingRadius > 0) 
        {
            Transform targetObject = FindClosestTargetInRange(targetDataRequest.TargetPos, targetDataRequest.HomingRadius);
            if (targetObject)
            {
                newTargData.SetObjectTarget(targetObject);
            }
        }

        return newTargData;
    }

    public Transform FindClosestTargetInRange(Vector3 searchPosition, float searchRadius, List<Collider2D> resultList = null)
    {
        if (resultList == null)
        {
            resultList = new List<Collider2D>();
        }
        FindTargetsInRange(searchPosition, searchRadius, resultList);
        if (resultList.Count > 0)
        {
            return resultList[0].transform;
        }
        else return null;
    }
    public List<Collider2D> FindTargetsInRange(Vector3 searchPosition, float searchRadius, List<Collider2D> resultList = null)
    {
        if (resultList == null)
        {
            resultList = new List<Collider2D>();
        }
        ContactFilter2D searchFilter = new ContactFilter2D();
        searchFilter.SetLayerMask(Character.find_character_mask);
        searchFilter.useLayerMask = true; // Actively use the mask
        Physics2D.OverlapCircle(searchPosition, searchRadius, searchFilter, resultList);
        return resultList;
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