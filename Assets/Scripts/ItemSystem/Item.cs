using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Compatibility;
using Unity.VisualScripting;
using UnityEngine;


public enum ItemType {Weapon, Support}
public class Item : MonoBehaviour
{   
    [field: SerializeField] public ItemSO baseData {get; private set;} // SO contains important base data
    public Character user;

    [SerializeField] private ItemEffect[] itemEffects = new ItemEffect[] {}; // determines the attacks available in this item
    public StackCounter[] stackCounters {get; private set;}

    [field: Header("Aiming")]
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90f); // RotateTowards() is stupid so we need to offset it
    public float _rotScale = 1f; // 0 for no rotation, 1 for instantaneous rotation
    Quaternion curr_rot; // save the current quaternion rotation
    public Vector2 aim_pos {get; private set;} // where the item is supposed to be aimed towards;
    public Vector2 source_pos {get; private set;} // where bullets & attacks originate from
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
    
    public float y_offset;

    // AIMING STUFF
    //public Vector2 target_pos {get; private set;} // where the item is actually aimed towards (based on _rotScale)
    private InputEventRelay externalInputRelay; // reference to another input relay which controls the item
    private InputEventRelay itemInputRelay; // a locally defined input relay

    [field: Header("Modifiers")]
    public float use_spd_scale = 1f;
    public float equip_spd_scale = 1f;
    public float resetSpdScale = 1f;

    #region Initializers

    public void Awake()
    {
        if (baseData)
        {
            Setup(baseData);
        }
        if (!itemTip)
        {
            itemTip = this.transform;
            y_offset = itemTip.transform.position.y - transform.position.y;
        }
    }

    public void Start()
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
        source_pos = transform.position + (curr_rot * Vector2.right);
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
            new (InputEvent, Type)[] {
                (InputEvent.Usable_Used, typeof(Action)),
                (InputEvent.Usable_ResetStart, typeof(Action)), 
                (InputEvent.Character_LookPos, typeof(Action<Vector2>)), 
            }
        );

        // find which stack counters use what inputs, update relay
        List<InputEvent> active_inputs = new List<InputEvent>();
        foreach(StackCounter counter in stackCounters)
        {
            counter.GetEvents(active_inputs);
            foreach(InputEvent input_enum in active_inputs)
            {
                itemInputRelay.AddEvent(input_enum, typeof(Action));
            }
            active_inputs.Clear();
            counter.SetInputRelay(itemInputRelay);
        }

        itemInputRelay.ConnectEvent(InputEvent.Usable_ResetStart, ResetItem);
        itemInputRelay.ConnectEvent(InputEvent.Character_LookPos, (Action<Vector2>)Aim);

        // set aiming type
        AimVFX = baseData.dynamic_aim ? DynamicAim : StaticAim;
        _rotScale = baseData.rotation_scale;
    }

    public void SetEquipped(bool is_equipped)
    {
        itemInputRelay.isActive = is_equipped;
    }
    // adjust item everytime theres a new user
    public void NewUser(InputEventRelay newInputRelay, Character new_char_user = null)
    {
        if (new_char_user)
            user = new_char_user;
        
        //unsubscribe from old user if they exist
        if (externalInputRelay != null)
            itemInputRelay.UnlinkRelay(externalInputRelay);
        
        // subscribe to old user events
        externalInputRelay = newInputRelay;
        if (externalInputRelay != null)
            itemInputRelay.LinkRelay(externalInputRelay);
    }
    public void UseItem(int attackIndex)
    {
        baseData.attackObjects[attackIndex].Attack(GetAttackTarget());
        itemInputRelay.Invoke(InputEvent.Usable_Used);
    }
    public void DropItem()
    {
        user = null;

    }

    // Calls item animator to reset the item
    public void ResetItem() // "reload" the item
    {
        Debug.Log("Resetting Item");
        bool can_reset = true;
        if (can_reset) {
            float reset_time = baseData.resetTime / resetSpdScale;
            animator.speed = 1/reset_time;
            animator.SetTrigger("Resetting");
            animator.ResetTrigger("Use");
        }
    }
    /// <summary>
    /// Called by the animator after the reset animator finishes
    /// reset data to original state
    /// </summary>
    public void ResetData() 
    {
        itemInputRelay.Invoke(InputEvent.Usable_ResetEnd);
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

    #region 
    public AttackTarget GetAttackTarget()
    {
        return new AttackTarget(user ? user.Position : this.transform.position, target_pos, itemTip.position, new Vector2(0, y_offset));
    }
    #endregion
}
