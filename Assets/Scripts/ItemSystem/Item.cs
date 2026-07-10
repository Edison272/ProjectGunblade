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

    [field: Header("VFX Body")]
    public GameObject itemobject;
    public Transform rotatorobject; // rotate the object when aiming
    public Transform itemTip; // the "front" of an item which attack type vfx will align to
    public Animator animator;
    // VFX STUFF
    
    public float y_offset;

    // AIMING STUFF
    public Vector2 target_pos {get; private set;} // where the item is actually aimed towards (based on rot_scale)
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
        itemInputRelay = new InputEventRelay();

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
    }

    public void SetEquipped(bool is_equipped)
    {
        itemInputRelay.isActive = is_equipped;
    }
    // adjust item everytime theres a new user
    public void NewUser(InputEventRelay newInputRelay)
    {
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
        return new AttackTarget(user ? user.GetPosition() : this.transform.position, target_pos, itemTip.position, new Vector2(0, y_offset));
    }
    #endregion
}
