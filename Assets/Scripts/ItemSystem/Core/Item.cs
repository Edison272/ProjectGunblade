using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;


public enum ItemType {Weapon, Support}

/*
The item class is the monobehaviour which manages core functionality- aiming, animations, and mechanics that all weapons & items have
*/
public class Item : MonoBehaviour
{   
    [field: SerializeField] public ItemSO baseData {get; private set;} // SO contains important base data
    public Character user;
    public Func<TargetDataRequest, TargetData> GetTargetData;

    [SerializeField] private ItemOutput[] _itemOutputs = new ItemOutput[] {}; // determines the attacks available in this item
    public StackCounter[] stackCounters {get; private set;}



    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float _rotScale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos {get; private set;} // where the item is supposed to be aimed towards;
    private Vector2 _sourcePos; // where bullets & attacks originate from
    public Vector2 targetPos {get; private set;} // where the item is actually aimed towards (based on _rotScale)
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    public delegate void AimDelegate();
    public AimDelegate AimVFX;

    [field: Header("VFX Body")]
    public GameObject itemobject;
    public Transform rotatorObject; // rotate the object when aiming
    public Transform itemTip; // the "front" of an item which attack type vfx will align to
    public Animator animator => GetComponent<Animator>();
    // VFX STUFF
    private float item_y_offset = 0f; // the y distance from item tip and item's main body position
    private float user_y_offset = 0f; // the y distance from the item's main body position to the user's position
    

    private InputEventRelay _externalInputRelay; // reference to another input relay which controls the item
    public InputEventRelay ItemInputRelay {get; private set;} // a locally defined input relay

    [field: Header("Modifiers")]
    public float use_spd_scale = 1f;
    public float equip_spd_scale = 1f;
    public float resetSpdScale = 1f;

    // animation speed stats
    public float EquipTime => baseData.EquipTime * equip_spd_scale;
    public float UnequipTime => baseData.UnequipTime * equip_spd_scale;
    public float ResetTime => baseData.ResetTime * resetSpdScale;

    // internal state data
    public bool IsInputsActive {get; private set;} = false; // toggled off/on when equipped/unequipped, and the item toggles select events
    
    // used by external scripts to check whether or not this item needs to be reset. this value is updated when the item is equippend, used, and reset. 
    // Readiness score is generally a time value- equip time, charge time, attack speed, whatever. The lower the time, the higher the score
    public float ReadinessScore {get; private set;} = 0; 
    private float _bestReadinessScore = 0; 

    private SortingGroup vfx_sorting => GetComponent<SortingGroup>(); // TURN OFF SORTING when picked up by a player or other parent object with sorting. only turn on sorting when in item form

    #region Intializer
    void Awake()
    {
        vfx_sorting.enabled = true;
        if (baseData)
        {
            Setup(baseData);
        }
        if (!itemTip)
        {
            itemTip = this.transform;
        }
        item_y_offset = itemTip.transform.position.y - transform.position.y;
    }

    public void Start()
    {
    }
    #endregion
    public void Update()
    {
        
    }
    #region Aiming
    public void Aim(Vector2 aim_pos)
    {
        this.aim_pos = new Vector2(aim_pos.x, aim_pos.y + user_y_offset);
        AimVFX();
        Vector2 aim_dir = aim_pos - (Vector2)transform.position;
        // the ACTUAL aiming aspect (get target position from aim_dir)
        Quaternion aim_rot = Quaternion.LookRotation(Vector3.forward, aim_dir) * ROTATION_OFFSET;
        curr_rot = Quaternion.Lerp(curr_rot, aim_rot, _rotScale);
        _sourcePos = transform.position + (curr_rot * Vector2.right);
        targetPos = transform.position + (curr_rot * Vector2.right * aim_dir.magnitude);
    }
    void StaticAim()
    {
        if((rotatorObject.transform.localScale.y >= 0) != (targetPos.x >= transform.position.x)) {
            Vector3 new_vec = rotatorObject.transform.localScale;
            new_vec.x *= -1;
            rotatorObject.transform.localScale = new_vec;
        }
    }
    void DynamicAim()
    {
        // set the item's rotation towards the target direction
        rotatorObject.transform.rotation = curr_rot;
        // make sure item scale is correct
        if((rotatorObject.transform.localScale.y >= 0) != (targetPos.x >= transform.position.x)) {
            Vector3 new_vec = rotatorObject.transform.localScale;
            new_vec.y *= -1;
            rotatorObject.transform.localScale = new_vec;
        }

    }

    #endregion
    #region Item Setup
    // Setup immutable item data when this object is made
    public void Setup(ItemSO baseData)
    {            
        // setup input relay stuff here
        ItemInputRelay = new InputEventRelay(
            new (Enum, Type)[] {
                (UsableEvent.Used, typeof(Action)),
                (UsableEvent.ResetStart, typeof(Action)), 
                (UsableEvent.ResetEnd, typeof(Action)), 
                (CharacterEvent.LookPos , typeof(Action<Vector2>)), 
            }
        );

        // deep copy of stack counters because these count for individual ites
        stackCounters = new StackCounter[baseData.StackCounters.Length];
        for (int i = 0; i < baseData.StackCounters.Length; i++)
        {
            stackCounters[i] = baseData.StackCounters[i].GetCopy();
        }
        // deep copy of item effects afterward (important)
        _itemOutputs = new ItemOutput[baseData.ItemOutputs.Length];
        for (int i = 0; i < baseData.ItemOutputs.Length; i++)
        {
            _itemOutputs[i] = baseData.ItemOutputs[i].GetCopy(this);
            
            float currVal = _itemOutputs[i].GetReadinessValue();
            if (currVal > _bestReadinessScore)
            {
                _bestReadinessScore = currVal;
            }

        }

        // find which stack counters use what inputs, update relay
        foreach(ItemOutput output in _itemOutputs)
        {
            ItemInputRelay.AddEvent(output.InputEvent.GetInputEvent(), typeof(Action));
            ItemInputRelay.AddEvent(output.OutputEvent.GetInputEvent(), typeof(Action));
            output.SetInputRelay(ItemInputRelay);
        }
        List<Enum> active_inputs = new List<Enum>();
        foreach(StackCounter counter in stackCounters)
        {
            counter.GetEvents(active_inputs);
            foreach(Enum input_enum in active_inputs)
            {
                ItemInputRelay.AddEvent(input_enum, typeof(Action));
            }
            active_inputs.Clear();
            counter.SetInputRelay(ItemInputRelay);
        }

        ItemInputRelay.ConnectEvent(UsableEvent.ResetStart, ResetItem);
        ItemInputRelay.ConnectEvent(CharacterEvent.LookPos, (Action<Vector2>)Aim);
        ItemInputRelay.IsActive = false;

        // set aiming type
        AimVFX = baseData.dynamic_aim ? DynamicAim : StaticAim;
        _rotScale = baseData.rotation_scale;
    }
    #endregion

    #region Equip/Unequip Control
    // method called externally to start the process of unequipping the item
    public void SetEquipped(bool is_equipped)
    {
        if (animator.GetBool("IsEquipped") == is_equipped)
            return;
        
        animator.speed = 1/EquipTime;
        if (!is_equipped)
        {
            animator.speed = 1/UnequipTime;
            animator.ResetTrigger("Resetting");
            animator.ResetTrigger("Use");
            SetItemInputRelay(false);
        }
        ReadinessScore = _bestReadinessScore;;
        animator.SetBool("IsEquipped", is_equipped); // call set equipped function through editor
    }

    private void SetItemInputRelay(bool isActive)
    {
        IsInputsActive = isActive;
        ItemInputRelay.SetEventActive(UsableEvent.ResetStart,isActive);
        ItemInputRelay.SetEventActive(UsableEvent.ResetEnd,isActive);

        ItemInputRelay.SetEventActive(CharacterEvent.MainStart,isActive);
        ItemInputRelay.SetEventActive(CharacterEvent.MainUpdate,isActive);
        ItemInputRelay.SetEventActive(CharacterEvent.MainEnd,isActive);
        ItemInputRelay.SetEventActive(CharacterEvent.AltStart,isActive);
        ItemInputRelay.SetEventActive(CharacterEvent.AltUpdate,isActive);
        ItemInputRelay.SetEventActive(CharacterEvent.AltEnd,isActive);
    }
    // Methods called by animator to actually equip the item and make it usuable
    private void AnimEquip(){SetItemInputRelay(true);}

    #endregion

    #region Set/Unset Item User
    // adjust item everytime theres a new user
    public void NewUser(InputEventRelay NewInputRelay, Character new_char_user)
    {
        NewUser(NewInputRelay, new_char_user.GetTargetData);
        user_y_offset = transform.position.y - new_char_user.Position.y;
    }
    public void NewUser(InputEventRelay NewInputRelay, Func<TargetDataRequest, TargetData> GetTargetFunc)
    {
        vfx_sorting.enabled = false;
        GetTargetData = GetTargetFunc;
    
        SetInputRelay(NewInputRelay);
        ItemInputRelay.IsActive = true;
        SetItemInputRelay(false);
    }
    private void SetInputRelay(InputEventRelay NewInputRelay)
    {
        //unsubscribe from old user if they exist
        if (_externalInputRelay != null) {
            Debug.Log("unlinking");
            ItemInputRelay.UnlinkRelay(_externalInputRelay);
        }
        // subscribe to old user events
        _externalInputRelay = NewInputRelay;
        if (_externalInputRelay != null)
            ItemInputRelay.LinkRelay(_externalInputRelay);
    }
    #endregion
    public void UseItem(int attackIndex, AnimationRequest animRequest, float readinessScore = -1)
    {
        // request targetting data from user
        TargetDataRequest targetDataReq = baseData.AttackObjects[attackIndex].GetTargetDataReq();
        targetDataReq.TargetPos = targetPos;
        
        // fill in the empty values
        TargetData get_atk_targ = GetTargetData(targetDataReq);
        get_atk_targ.targetPos = targetPos;
        get_atk_targ.vfxSourcePos = itemTip.transform.position;
        get_atk_targ.vfxTargetOffset = new Vector2(0, item_y_offset + user_y_offset);
        
        
        baseData.AttackObjects[attackIndex].Attack(get_atk_targ);
        animRequest.Animate(animator);
        ItemInputRelay.Invoke(UsableEvent.Used);

        if (readinessScore > -1)
            ReadinessScore = readinessScore;
    }


    #region Reset Item / Data

    // Calls item animator to reset the item
    public void ResetItem() // "reload" the item
    {
        SetItemInputRelay(false);
        ItemInputRelay.Invoke(UsableEvent.ResetStart);
        animator.speed = 1/ResetTime;
        animator.ResetTrigger("Use");
        animator.SetTrigger("Resetting");
    }
    /// <summary>
    /// Called by the animator after the reset animator finishes
    /// </summary>
    public void AnimResetItem() 
    {
        ItemInputRelay.Invoke(UsableEvent.ResetEnd);
        SetItemInputRelay(true);
        ReadinessScore = _bestReadinessScore;
        // use_spd_scale = 1;
        // reset_spd_scale = 1;
        // equip_spd_scale = 1;

        // // reset animator state
        // animator.Rebind();
        // animator.Update(0f);

        // is_equipped = false;
        // reset_timer = 0;

        // input_controller.Reset();
        // functionality_controller.ResetData();
    }
    #endregion

    #region Getting Data

    #endregion
}
