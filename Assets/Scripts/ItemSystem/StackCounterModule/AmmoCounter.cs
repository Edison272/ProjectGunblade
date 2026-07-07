using System;
using System.Collections;
using System.Collections.Generic;
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
    public int maxAmmo;
    public int curr_ammo;
    public float reloadSpeed;
    public InputEvent UseAmmoEvent = InputEvent.MainStart;
    public InputEvent ReloadEvent = InputEvent.Reset;

    [Header("Regen Ammo")] // instead of manually reloading, slowly reloads ammo when not being used
    public bool regenAmmo;
    [ShowIf("regenAmmo")] public RegenAmmoModule regenAmmoModule;

    [Header("Rounds Reload")] // instead of fully reloading all ammo at once, ammo is reloaded in chunks
    public bool roundsReload;
    [ShowIf("roundsReload")]  public RoundsReloadModule roundsReloadModule;

    #region Ammo Substats
    public struct RegenAmmoModule
    {
        public float delay;
    }
    public struct RoundsReloadModule
    {
        public int rounds_per_load;
    }
    #endregion

    #region Initalizers
    public AmmoCounter()
    {
        stackCounterType = StackCountType.Ammo;
    }
    // creates a deepy copy of this class.
    public override StackCounter GetCopy()
    {
        return new AmmoCounter();
    }
    #endregion

    #region Functionality
    public void UseCounter(int stack_usage = -1)
    {
        curr_ammo += stack_usage;
        curr_ammo = Math.Clamp(curr_ammo, 0, maxAmmo);
    }
    public void ReloadAmmo(int amt = 0)
    {
        amt = amt == 0 ? maxAmmo : amt; 
    }

    public override void GetEvents(List<Enum> inputEventsUsed)
    {
        inputEventsUsed.Add(UseAmmoEvent);
        inputEventsUsed.Add(ReloadEvent);
    }
    #endregion
    
    #region Stack Status
    public override float GetStatus()
    {
        return (float)curr_ammo/maxAmmo;
    }
    #endregion




}
