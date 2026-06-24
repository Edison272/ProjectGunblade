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

    public ItemEffect[] ItemEffects;
    // Input Instance




    #region Initializers
    // Setup immutable item data when this object is made
    public void Setup(ItemSO base_data)
    {            

    }

    // adjust item everytime theres a new user
    public void NewUser(Character new_user)
    {
        //unsubscribe from old user if they exist

        // subscribe to old user events
        user = new_user;
        foreach (ItemEffect effect in ItemEffects)
        {
            effect.AssignInputs(new_user);
        }

        new_user.ResetEvent += ResetItem;
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
