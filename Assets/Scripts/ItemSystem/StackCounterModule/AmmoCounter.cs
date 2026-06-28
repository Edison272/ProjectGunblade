using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// ammo counter types are as the name implies, ammo. 
/// They are usable when the amount of stacks > 1, and must reload ammo when stacks are 0
/// Different features will be able to change how ammo is spent/reloaded
/// </summary>
[Serializable]
public class AmmoCounter : StackCounter
{
    [Header("Basic Stats")]
    public int max_ammo;
    public int curr_ammo;
    public float reload_speed;

    [Header("Regen Ammo")] // instead of manually reloading, slowly reloads ammo when not being used
    public bool regen_ammo;
    //[ShowIf("regen_ammo")] public RegenAmmoModule regen_ammo_modifier;

    [Header("Rounds Reload")] // instead of fully reloading all ammo at once, ammo is reloaded one by one
    public bool rounds_reload;
    [ShowIf("rounds_reload")] public int rounds_per_load = 1;

    #region Initalizer
    public AmmoCounter()
    {
        stackCounterType = StackCountType.Ammo;
    }
    #endregion

    #region Functionality
    public void UseCounter(int stack_usage = -1)
    {
        curr_ammo += stack_usage;
        curr_ammo = Math.Clamp(curr_ammo, 0, max_ammo);
    }


    #endregion
    
    #region Stack Status
    public override bool IsReady()
    {
        return curr_ammo > max_ammo;
    }

    public override float GetStatus()
    {
        return (float)curr_ammo/max_ammo;
    }
    #endregion

}