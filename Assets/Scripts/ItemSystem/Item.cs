using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;


public enum ItemType {Weapon, Support}
public class Item : MonoBehaviour
{   
    [field: SerializeField] public ItemSO baseData {get; private set;} // SO contains important base data
    public Character user;
    public Func<TargetData> GetTargetData;

    [SerializeField] private ItemEffect[] itemEffects = new ItemEffect[] {}; // determines the attacks available in this item
    public StackCounter[] stackCounters {get; private set;}



    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float _rotScale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos {get; private set;} // where the item is supposed to be aimed towards;
    [SerializeField] private Vector2 _outputPos; // where bullets & attacks originate from
    public Vector2 target_pos {get; private set;} // where the item is actually aimed towards (based on _rotScale)
    bool freeze_aiming = false;     // stop this thing from aiming and updating target position
    public delegate void AimDelegate();
    public AimDelegate AimVFX;

    [field: Header("VFX Body")]
    public GameObject itemobject;
    public Transform rotatorObject; // rotate the object when aiming
    public Transform itemTip; // the "front" of an item which attack type vfx will align to
    public Animator animator;
    // VFX STUFF
    private float item_y_offset = 0f; // the y distance from item tip and item's main body position
    private float user_y_offset = 0f; // the y distance from the item's main body position to the user's position
    


    private InputEventRelay externalInputRelay; // reference to another input relay which controls the item
    private InputEventRelay itemInputRelay; // a locally defined input relay

    [field: Header("Modifiers")]
    public float use_spd_scale = 1f;
    public float equip_spd_scale = 1f;
    public float resetSpdScale = 1f;

    // animation speed stats
    public float EquipTime => baseData.EquipTime * equip_spd_scale;
    public float UnequipTime => baseData.UnequipTime * equip_spd_scale;
    public float ResetTime => baseData.ResetTime * resetSpdScale;

    // internal state data
    private bool _isInputsActive = false; // toggled off/on when equipped/unequipped, and the item toggles select events

    private SortingGroup vfx_sorting; // TURN OFF SORTING when picked up by a player or other parent object with sorting. only turn on sorting when in item form

    #region Intializer
    void Awake()
    {
        vfx_sorting = transform.GetComponent<SortingGroup>();
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
        this.aim_pos = aim_pos;
        AimVFX();
        Vector2 aim_dir = aim_pos - (Vector2)transform.position;
        // the ACTUAL aiming aspect (get target position from aim_dir)
        Quaternion aim_rot = Quaternion.LookRotation(Vector3.forward, aim_dir) * ROTATION_OFFSET;
        curr_rot = Quaternion.Lerp(curr_rot, aim_rot, _rotScale);
        _outputPos = transform.position + (curr_rot * Vector2.right);
        target_pos = transform.position + (curr_rot * Vector2.right * aim_dir.magnitude);
    }
    void StaticAim()
    {
        if((rotatorObject.transform.localScale.y >= 0) != (target_pos.x >= transform.position.x)) {
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
        if((rotatorObject.transform.localScale.y >= 0) != (target_pos.x >= transform.position.x)) {
            Vector3 new_vec = rotatorObject.transform.localScale;
            new_vec.y *= -1;
            rotatorObject.transform.localScale = new_vec;
        }

    }

    #endregion

    // Setup immutable item data when this object is made
    public void Setup(ItemSO baseData)
    {            
        // build functionality from SO

        // deep copy of stack counters because these count for individual ites
        stackCounters = new StackCounter[baseData.stackCounters.Length];
        for (int i = 0; i < baseData.stackCounters.Length; i++)
        {
            stackCounters[i] = baseData.stackCounters[i].GetCopy();
        }
        // deep copy of item effects afterward (important)
        itemEffects = new ItemEffect[baseData.itemEffects.Length];
        for (int i = 0; i < baseData.itemEffects.Length; i++)
        {
            itemEffects[i] = baseData.itemEffects[i].GetCopy(this);
        }

        // setup input relay stuff here
        itemInputRelay = new InputEventRelay(
            new (Enum, Type)[] {
                (UsableEvent.Used, typeof(Action)),
                (UsableEvent.ResetStart, typeof(Action)), 
                (UsableEvent.ResetEnd, typeof(Action)), 
                (CharacterEvent.LookPos , typeof(Action<Vector2>)), 
            }
        );

        // find which stack counters use what inputs, update relay
        List<Enum> active_inputs = new List<Enum>();
        foreach(StackCounter counter in stackCounters)
        {
            counter.GetEvents(active_inputs);
            foreach(Enum input_enum in active_inputs)
            {
                itemInputRelay.AddEvent(input_enum, typeof(Action));
            }
            active_inputs.Clear();
            counter.SetInputRelay(itemInputRelay);
        }

        itemInputRelay.ConnectEvent(UsableEvent.ResetStart, ResetItem);
        itemInputRelay.ConnectEvent(CharacterEvent.LookPos, (Action<Vector2>)Aim);
        itemInputRelay.IsActive = false;

        // set aiming type
        AimVFX = baseData.dynamic_aim ? DynamicAim : StaticAim;
        _rotScale = baseData.rotation_scale;
    }

    #region Equip/Unequip Control
    // method called externally to start the process of unequipping the item
    public void SetEquipped(bool is_equipped)
    {
        animator.speed = 1/EquipTime;
        if (!is_equipped)
        {
            animator.speed = 1/UnequipTime;
            animator.SetTrigger("Cancel");
            animator.ResetTrigger("Resetting");
            animator.ResetTrigger("Use");
            SetItemInputRelay(false);
        }
        animator.SetBool("IsEquipped", is_equipped); // call set equipped function through editor
    }

    private void SetItemInputRelay(bool isActive)
    {
        _isInputsActive = isActive;
        itemInputRelay.SetEventActive(UsableEvent.ResetStart,isActive);
        itemInputRelay.SetEventActive(UsableEvent.ResetEnd,isActive);

        itemInputRelay.SetEventActive(CharacterEvent.MainStart,isActive);
        itemInputRelay.SetEventActive(CharacterEvent.MainUpdate,isActive);
        itemInputRelay.SetEventActive(CharacterEvent.MainEnd,isActive);
        itemInputRelay.SetEventActive(CharacterEvent.AltStart,isActive);
        itemInputRelay.SetEventActive(CharacterEvent.AltUpdate,isActive);
        itemInputRelay.SetEventActive(CharacterEvent.AltEnd,isActive);
    }
    // Methods called by animator to actually unequip the item and make it unusuable
    private void AnimEquip(){SetItemInputRelay(true);}

    #endregion

    #region Set/Unset Item User
    // adjust item everytime theres a new user
    public void NewUser(InputEventRelay NewInputRelay, Character new_char_user)
    {
        NewUser(NewInputRelay, new_char_user.GetTargetData);
        user_y_offset = transform.position.y - new_char_user.Position.y;
    }
    public void NewUser(InputEventRelay NewInputRelay, Func<TargetData> GetTargetFunc)
    {
        vfx_sorting.enabled = false;
        GetTargetData = GetTargetFunc;
    
        SetInputRelay(NewInputRelay);
        itemInputRelay.IsActive = true;
        SetItemInputRelay(false);
    }
    private void SetInputRelay(InputEventRelay NewInputRelay)
    {
        //unsubscribe from old user if they exist
        if (externalInputRelay != null) {
            Debug.Log("unlinking");
            itemInputRelay.UnlinkRelay(externalInputRelay);
        }
        // subscribe to old user events
        externalInputRelay = NewInputRelay;
        if (externalInputRelay != null)
            itemInputRelay.LinkRelay(externalInputRelay);
    }
    #endregion
    public void UseItem(int attackIndex, AnimationRequest animRequest)
    {
        // fill in the empty values
        TargetData get_atk_targ = GetTargetData();
        get_atk_targ.target_pos = target_pos;
        get_atk_targ.output_pos = _outputPos;
        get_atk_targ.vfx_target_offset = new Vector2(0, item_y_offset + user_y_offset);
        
        
        baseData.attackObjects[attackIndex].Attack(get_atk_targ);
        animRequest.Animate(animator);
        itemInputRelay.Invoke(UsableEvent.Used);
    }
    public void DropItem()
    {
        user = null;

    }


    #region Reset Item / Data
    // Calls item animator to reset the item
    public void ResetItem() // "reload" the item
    {

        if (!_isInputsActive) {return;}
        SetItemInputRelay(false);
        itemInputRelay.Invoke(UsableEvent.ResetStart);
        animator.speed = 1/ResetTime;
        animator.ResetTrigger("Use");
        animator.SetTrigger("Cancel");
        animator.SetTrigger("Resetting");
    }
    /// <summary>
    /// Called by the animator after the reset animator finishes
    /// </summary>
    public void AnimResetItem() 
    {
        itemInputRelay.Invoke(UsableEvent.ResetEnd);
        SetItemInputRelay(true);
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

    // cancels all ongoing invokes
    public void InterruptItem()
    {
        
    }
    #endregion


}
