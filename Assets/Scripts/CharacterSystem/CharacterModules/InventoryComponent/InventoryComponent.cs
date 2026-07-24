using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// An inventory system. Can be used for items, or for abilities. But items for now.
/// use different data structures to contain indexes to different parts of the main inventory list
/// </summary>
public class InventoryComponent
{
    public Item[] inventory;
    public InventorySlot[] inventorySlots;  // The actual interactable way to use Items by accessing indexes in the inventory
    protected int _currSlotIndex = 0;  // access items indexes list
    public InventorySlot CurrentSlot {get; protected set;}
    protected float switch_cd = 1.5f; // time the char must wait before they can switch to the next weapon
    protected float curr_switch_cd = 0;
    private int curr_capacity = 0;
    private Character _character;
    public Delegate SwitchItemCycle {get; private set;}

    #region Intializer
    // internal setup class
    public InventoryComponent(CharacterSO baseData, Character character)
    {
        ItemSO[] initialized_inventory = baseData.inventory;
        InventorySlot[] initialized_active_slots = baseData.inventory_slots;
        _character = character;
        
        // sets inventory size to holding capacity, but will set to the init inventory if it's bigger
        int holding_capacity = Mathf.Max(initialized_inventory.Length, baseData.HoldingCapacity);
        int item_slots = Mathf.Max(initialized_active_slots.Length, baseData.TotalItemSlots);
        inventory = new Item[holding_capacity];
        inventorySlots = new InventorySlot[initialized_active_slots.Length];

        // initialize default items to the main hand by default
        for(int i = 0; i < initialized_inventory.Length; i++)
        {
            inventory[i] = GetNewItem(initialized_inventory[i], true);
        }

        // sort out where each items go in the slots
        for(int i = 0; i < initialized_inventory.Length; i++)
        {
            inventorySlots[i] = new InventorySlot(initialized_active_slots[i]).SetInventoryReference(inventory);
        }
        
        // default to first weapon in inventory
        _currSlotIndex = 0;
        CurrentSlot = inventorySlots[0];
        curr_switch_cd = CurrentSlot.EquipSpeed; // set timer before equipping new weapons

        SwitchItemCycle = (Action)(() => SwitchItem(-1));
    }
    #endregion

    #region Updates
    public void Update()
    {
        // keep track of timer to equip weapons based on switch time
        if (curr_switch_cd > 0)
        {
            curr_switch_cd -= Time.deltaTime;
            if (curr_switch_cd <= 0)
            {
                SetInventorySlot(true);
            }
        }
    }
    #endregion

    #region Equip/Unequip
    // cycle between inventorySlots slots, or choose a select slot with spec_index
    // parameer -1 to do a basic cycle of through the inventory
    public void SwitchItem(int spec_index = -1) 
    {
        if (spec_index == _currSlotIndex) {return;} // dont do anything if switching to active items
        
        if (spec_index == -1) // typical incrementation
        {
            _currSlotIndex += 1;
            if (_currSlotIndex > inventorySlots.Length - 1)
            {
                _currSlotIndex = 0;
            }
        }
        else // specific index
        {
            _currSlotIndex = Mathf.Clamp(spec_index, 0, inventorySlots.Length);
        }
        // the actual "switching" part which sets the current active slot
        curr_switch_cd = CurrentSlot.UnequipSpeed; // set timer before equipping new weapons
        Debug.Log(Time.time);
        SetInventorySlot(false); //unequipped item will call the "SetSwitchItem" in animator to set the new active item
        CurrentSlot = inventorySlots[_currSlotIndex];
        
    }

    void SetInventorySlot(bool is_active) // unequip or equip the active slot
    {
        Debug.Log(Time.time);
        if (is_active)
        {
            _character.Anatomy.SetAimStyle(CurrentSlot.IsAkimbo); // adjust how the item(s) look in the player's hands
            CurrentSlot.SetSlotAcive(true);
        }
        else
        {
            CurrentSlot.SetSlotAcive(false);
        }
        
    }

    #endregion
    public Item GetNewItem(ItemSO itemBase, bool onAltHand = false)
    {
        Item newItem = itemBase.GenerateItem(_character.Anatomy.GetHand(onAltHand));
        return SetItemUser(newItem);
    }
    public Item SetItemUser(Item newItem)
    {
        newItem.NewUser(_character.characterRelay, _character);
        return newItem;
    }

    #region Pickup/Drop

    // Called by character when they pickup a new item or it's added to their inventory
    public Item PickupItem(Item new_item, bool setAsAlt = false)
    {
        Item switch_out_item = null;
        Debug.Log(new_item);
        // simply add the item in if there's still space
        if (curr_capacity < inventory.Count())
        {
            inventory[curr_capacity] = new_item;
            curr_capacity++;
        }
        // switch out item slot if inventory is full
        else
        {
            int itemSlot = setAsAlt ? CurrentSlot.AltSlot : CurrentSlot.MainSlot;
            switch_out_item = inventory[itemSlot];
            inventory[itemSlot] = new_item;
        }
        new_item.transform.parent = _character.Anatomy.GetHand(setAsAlt);
        new_item.transform.localPosition = Vector3.zero;
        float scale = new_item.transform.localScale.x;
        new_item.transform.localScale = new Vector3(Mathf.Abs(scale), Mathf.Abs(scale), Mathf.Abs(scale));
        SetItemUser(new_item); // set up the new shi

        return switch_out_item;
    }

    public void DropItem(Item dropItem)
    {
        
    }

    #endregion

}