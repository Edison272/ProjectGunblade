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
[Serializable]
public class InventoryComponent
{
    public Item[] inventory;
    public InventorySlot[] inventorySlots;  // The actual interactable way to use Items by accessing indexes in the inventory
    protected int _currSlotIndex = 0;  // access items indexes list
    public InventorySlot CurrentSlot {get; protected set;}
    // each item slot  reserves a slot in the inventory for themselves
    protected float switch_cd = 1.5f; // time the char must wait before they can switch to the next weapon
    protected float curr_switch_cd = 0;
    private Character _character;
    public Delegate SwitchItemCycle {get; private set;}

    #region Intializer
    // internal setup class
    public InventoryComponent(CharacterSO baseData, Character character)
    {
        _character = character;
        SetupInventory(baseData.inventory, baseData.inventory_slots, baseData.HoldingCapacity);
    }

    public void SetupInventory(ItemSO[] initialized_inventory, InventorySlot[] initialized_active_slots, int HoldingCapacity)
    {
        // sets inventory size to holding capacity, but will set to the init inventory if it's bigger
        int holding_capacity = Mathf.Max(initialized_inventory.Length, HoldingCapacity);
        inventory = new Item[holding_capacity];
        inventorySlots = new InventorySlot[initialized_active_slots.Length];
        
        for(int i = 0; i < initialized_active_slots.Length; i++)
        {
            inventorySlots[i] = new InventorySlot(initialized_active_slots[i]).SetInventoryReference(inventory);

        }
        // initialize default items to the main hand by default
        for(int i = 0; i < initialized_inventory.Length; i++)
        {
            inventory[i] = GetNewItem(initialized_inventory[i], true);
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
        curr_switch_cd = CurrentSlot.UnequipSpeed + 0.0001f; // set timer before equipping new weapons
        SetInventorySlot(false); //unequipped item will call the "SetSwitchItem" in animator to set the new active item
        CurrentSlot = inventorySlots[_currSlotIndex];
        
    }

    void SetInventorySlot(bool is_active) // unequip or equip the active slot
    {
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
        // setup the item and where it's gonna be held
        new_item.transform.parent = _character.Anatomy.GetHand(!setAsAlt);
        new_item.transform.localPosition = Vector3.zero;
        float scale = new_item.transform.localScale.x;
        new_item.transform.localScale = new Vector3(Mathf.Abs(scale), Mathf.Abs(scale), Mathf.Abs(scale));
        SetItemUser(new_item); // set up the new shi
        
        
        Item switch_out_item = null;
        // simply add the item in if there's still an empty slot
        int setSlotIdx = FindEmptySlot();
        if (setSlotIdx > -1)
        {
            Debug.Log($"taking vacancy at {setSlotIdx}");
            InventorySlot emptySlot = inventorySlots[setSlotIdx];
            int newItemIdx = FindEmptyInventoryIndex();
            inventory[newItemIdx] = new_item;
            if (!setAsAlt)
                emptySlot.SetMainIndex(newItemIdx);
            else
                emptySlot.SetAltIndex(newItemIdx);

            inventorySlots[setSlotIdx] = emptySlot;
            SwitchItem(setSlotIdx);
        }
        // switch out current item slot if inventory is full
        else
        {
            int itemSlotIndex = 0;
            if (setAsAlt)
                itemSlotIndex = CurrentSlot.AltSlot;
                if (itemSlotIndex == -1)
                {
                    itemSlotIndex = FindEmptyInventoryIndex();
                    CurrentSlot.SetAltIndex(itemSlotIndex);
                }
            else
                itemSlotIndex = CurrentSlot.MainSlot;
            switch_out_item = inventory[itemSlotIndex];
            switch_out_item?.SetEquipped(false);
            inventory[itemSlotIndex] = new_item;
            SetInventorySlot(true);
        }

        return switch_out_item;
    }

    public void DropItem(Item dropItem)
    {
        
    }
    #endregion

    #region Getting Data
    // gets the current slot's readiness score
    public float GetActiveSlotReadiness()
    {
        return CurrentSlot.GetSlotReadiness();
    }
    
    // gets the slot most "ready"
    public float GetHighestReadinessSlot()
    {
        return CurrentSlot.GetSlotReadiness();
    }

    #endregion

    #region Helpers
    // starting from 0, find a slot without anything. Otherwise return -1 to signify no empty slots were found
    public int FindEmptySlot()
    {
        for(int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i].IsEmpty)
            {
                return i;
            }
        }
        return -1;
    }
    public int FindSlot()
    {
        for(int i = 0; i < inventorySlots.Length; i++)
        {
            if (!inventorySlots[i].IsEmpty)
            {
                return i;
            }
        }
        return -1;
    }
    public int FindEmptyInventoryIndex()
    {
        for(int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == null)
            {
                return i;
            }
        }
        return -1;
    }

    #endregion

}