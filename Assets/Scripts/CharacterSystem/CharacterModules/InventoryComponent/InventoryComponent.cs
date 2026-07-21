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
    public List<Item> inventory;
    public List<ActiveSlot> inventorySlots;  // Access items from the items list with indexes. Vector X for Main Item, Vector Y for Alt Item
    protected int _currSlotIndex = 0;  // access items indexes list
    public ActiveSlot CurrentSlot {get; protected set;}
    protected float switch_cd = 1.5f; // time the char must wait before they can switch to the next weapon
    protected float curr_switch_cd = 0;
    private int holding_capacity = 0;
    private Character _character;
    public Delegate SwitchItemCycle {get; private set;}

    #region Intializer
    // internal setup class
    public InventoryComponent(CharacterSO baseData, Character character)
    {
        ItemSO[] initialized_inventory = baseData.inventory;
        ActiveSlot[] initialized_active_slots = baseData.inventory_slots;
        _character = character;
        
        // // setup inventory
        Item[] init_inventory = new Item[initialized_inventory.Length];
        inventory = new List<Item>();
        inventorySlots = new List<ActiveSlot>();
        foreach(ActiveSlot slot in initialized_active_slots)
        {
            ActiveSlot new_slot = new ActiveSlot(slot).SetInventoryReference(inventory);
            inventorySlots.Add(new_slot);

            // initializes items based on the slots list. does not initialize the item if something already exists at the designated area
            if (!init_inventory[new_slot.MainSlot]) 
            {
                init_inventory[new_slot.MainSlot] = GetNewItem(initialized_inventory[new_slot.MainSlot], true);
            }
            else {Debug.LogWarning($"Item already occupying the main spot at index slot {new_slot.MainSlot} ! new item will not be initialzied");}

            if (new_slot.AltSlot > -1)
            {
                if (!init_inventory[new_slot.AltSlot])
                {
                    init_inventory[new_slot.AltSlot] = GetNewItem(initialized_inventory[new_slot.AltSlot], false);
                }
                else
                {
                    Debug.LogWarning($"Item already occupying the alt spot at index slot {new_slot.AltSlot} ! new item will not be initialzied");
                }
            }
        }

        foreach(Item newItem in init_inventory)
        {
            inventory.Add(newItem);
        }
        
        holding_capacity = Mathf.Max(inventorySlots.Count, baseData.holding_capacity);

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
                SetActiveSlot(true);
            }
        }
    }
    #endregion

    #region Equip/Unequip
    // cycle between inventorySlots slots, or choose a select slot with spec_index
    // parameer -1 to do a basic cycle of through the inventory
    public void SwitchItem(int spec_index) 
    {
        if (spec_index == _currSlotIndex) {return;} // dont do anything if switching to active items
        
        if (spec_index == -1) // typical incrementation
        {
            _currSlotIndex += 1;
            if (_currSlotIndex > inventorySlots.Count - 1)
            {
                _currSlotIndex = 0;
            }
        }
        else // specific index
        {
            _currSlotIndex = Mathf.Clamp(spec_index, 0, inventorySlots.Count);
        }
        // the actual "switching" part which sets the current active slot
        curr_switch_cd = CurrentSlot.UnequipSpeed; // set timer before equipping new weapons
        SetActiveSlot(false); //unequipped item will call the "SetSwitchItem" in animator to set the new active item
        CurrentSlot = inventorySlots[_currSlotIndex];
        
    }

    void SetActiveSlot(bool is_active) // unequip or equip the active slot
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
        Item new_item = itemBase.GenerateItem(_character.Anatomy.GetHand(onAltHand));
        new_item.NewUser(_character.characterRelay, _character);
        return new_item;
    }


    // public Item PickupItem(Item new_item)
    // {
    //     Item switch_out_item = null;
    //     if (inventorySlots.Count >= holding_capacity)
    //     {            
    //         // switch out current item with the new pickup
    //         switch_out_item = inventory[current_indexes.Item1];
    //         inventory[current_indexes.Item1] = new_item;

    //         new_item.transform.parent = main_hand;
    //         new_item.transform.localPosition = Vector3.zero;
    //         float scale = new_item.transform.localScale.x;
    //         new_item.transform.localScale = new Vector3(Mathf.Abs(scale), Mathf.Abs(scale), Mathf.Abs(scale));
    //         new_item.NewUser(this);
            
    //         EquipActive(_currSlotIndex); // set up the new shi
    //         curr_switch_cd = switch_cd;
    //     } 
    //     else
    //     {
    //         AddItem(new_item);
    //     }
    //     return switch_out_item;
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
    //     inventorySlots.Add(new Vector2Int(inventory.Count-1, -1));

    //     new_item.NewUser(this);
    //     new_item.UnequipItem();

    //     return inventorySlots.Count-1;
    // }


}