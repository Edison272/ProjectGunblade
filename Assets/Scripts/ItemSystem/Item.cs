using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum ItemType {Weapon, Support}
public class Item : MonoBehaviour
{   
    [field: SerializeField] public ItemSO base_data {get; private set;} // SO contains important base data
    public Character user;

    private ItemEffect[] itemEffects = new ItemEffect[] {}; // determines the attacks available in this item
    private StackCounter[] stackCounters; // determines the resources the attacks relies on / update
    public InputEventRelay inputRelay; // reference to another input relay

    #region Initializers

    public void Awake()
    {
    }

    public void Start()
    {
        
    }

    // Setup immutable item data when this object is made
    public void Setup(ItemSO base_data)
    {            
        // build functionality from SO
        itemEffects = base_data.itemEffects;

        // Deep copy of stack counters
        int counter_amt= base_data.stackCounters.Length;
        stackCounters = new AmmoCounter[counter_amt];
        for (int i = 0; i < counter_amt; i++)
        {
            stackCounters[i] = base_data.stackCounters[i].GetCopy();
        }
    }

    // adjust item everytime theres a new user
    public void NewUser(InputEventRelay newInputRelay)
    {
        //unsubscribe from old user if they exist

        // subscribe to old user events
        inputRelay = newInputRelay;
        inputRelay.ConnectEvent(InputEvent.MainStart, () => {Debug.Log($"main start");});
        inputRelay.ConnectEvent(InputEvent.MainEnd, () => {Debug.Log($"main end");});
        inputRelay.ConnectEvent(InputEvent.AltStart, () => {Debug.Log($"alt start");});
        inputRelay.ConnectEvent(InputEvent.AltEnd, () => {Debug.Log($"alt end");});
        inputRelay.ConnectEvent(InputEvent.Reset, () => {Debug.Log($"reset");});

        // foreach (ItemEffect effect in itemEffects)
        // {
        //     effect.SetupEventListeners(new_user);
        // }

        // new_user.ResetEvent += ResetItem;
    }
    public void DropItem()
    {
        user = null;

    }

    public void ResetItem() // "reload" the item
    {
        
    }

    public void ResetData() // reset data to original state
    {
        Debug.Log("Resetting Item");
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
}
