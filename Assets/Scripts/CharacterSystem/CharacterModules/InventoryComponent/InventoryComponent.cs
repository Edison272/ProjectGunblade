using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An inventory system. Can be used for items, or for abilities. But items for now.
/// use different data structures to contain indexes to different parts of the main inventory list
/// </summary>
public class InventoryComponent
{
    public List<Item> inventory;
    public List<ActiveSlot> item_indexes;  // Access items from the items list with indexes. Vector X for Main Item, Vector Y for Alt Item
    protected int curr_item_index = 0;  // access items indexes list
    public ActiveSlot current_indexes {get; protected set;}
    protected float switch_cd = 0.5f; // time the char must wait before they can switch to the next weapon
    protected float curr_switch_cd = 0;
    private int holding_capacity = 0;
    public InventoryComponent(ItemSO[] initialized_inventory, ActiveSlot[] initialized_active_slots, int holding_capacity)
    {
        // // setup inventory
        Item[] init_inventory = new Item[initialized_inventory.Length];
        ActiveSlot[] init_item_indexes = new ActiveSlot[initialized_active_slots.Length];
        for (int i = 0; i < initialized_active_slots.Length; i++)
        {
            int main_item = initialized_active_slots[i].MainSlot;
            int alt_item = initialized_active_slots[i].AltSlot;
            init_item_indexes[i] = initialized_active_slots[i];
            // init_inventory[main_item] = GetItemSO(initialized_inventory[main_item]);
            // if (alt_item > -1)
            // {
            //     init_inventory[alt_item] = GetItemSO(initialized_inventory[alt_item], true);
            // }
        }
        inventory = init_inventory.ToList<Item>();
        item_indexes = init_item_indexes.ToList<ActiveSlot>();
        this.holding_capacity = Mathf.Max(item_indexes.Count, holding_capacity);
    }
    #region  Inventory Management
    public bool HasAltAction() // returns true if the operator is currently wielding two items or an multi-state items
    {
        return item_indexes[curr_item_index].AltSlot != -1;  // return true for one action, and false for two actions
    }
    #endregion

    #region Equip/Unequip
    void EquipActive(int index)  // equip the currently selected weapons (the ones the operator is currently holding)
    {
        // main_item = inventory[item_indexes[index].MainSlot];
        // if (item_indexes[index].y == -1)
        // {
        //     alt_item = null;
        // } 
        // else
        // {
        //     alt_item = inventory[item_indexes[index].y];
        // }
        //curr_range = base_range + GetRangeScalar();
    }
    void UnequipActive() // unequip animation for the currently selected weapons
    {
        // main_item.SetEquipped(false);
        // alt_item?.SetEquipped(false);
    }
    public void SetSwitchItem() // only setup the new item VFX after the old one has been put away completely
    {
        // main_item.SetEquipped(true);
        // alt_item?.SetEquipped(true);
        // CharacterAnatomy.SetAimStyle(alt_item); // adjust how the item(s) look in the player's hands
    }

    #endregion
    // public Item GetItemSO(ItemSO new_item, bool on_main_hand = true)
    // {
        // Transform hand_hold = CharacterAnatomy.GetHand(on_main_hand);
        // return new_item.GenerateItem(hand_hold);
    //}
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
    
    public void SwitchItem(int spec_index = -1) // cycle between item_indexes slots, or choose a select slot with spec_index
    {
        if (spec_index == curr_item_index) {return;} // dont do anything if switching to active items
        
        if (spec_index == -1) // typical incrementation
        {
            curr_item_index += 1;
            if (curr_item_index > item_indexes.Count - 1)
            {
                curr_item_index = 0;
            }
        }
        else // specific index
        {
            curr_item_index = Mathf.Clamp(spec_index, 0, item_indexes.Count);
        }
        UnequipActive(); //unequipped item will call the "SetSwitchItem" in animator to set the new active item
        // current_indexes = (item_indexes[curr_item_index].x, item_indexes[curr_item_index].y);
        // EquipActive(curr_item_index); // set up the new shi
        // curr_switch_cd = switch_cd; // set timer before equipping new weapons
    }
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


}