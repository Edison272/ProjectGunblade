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
    public BasicAmmoStats basicAmmoStats;
        public int maxAmmo => basicAmmoStats.maxAmmo;
        public int curr_ammo => basicAmmoStats.curr_ammo;
        public float reloadSpeed => basicAmmoStats.reloadSpeed;
        public InputEvent UseAmmoEvent => basicAmmoStats.UseAmmoEvent;
        public InputEvent ReloadEvent => basicAmmoStats.ReloadEvent;

    [Serializable]
    public struct BasicAmmoStats
    {
        public int maxAmmo;
        public int curr_ammo;
        public float reloadSpeed;
        public InputEvent UseAmmoEvent;
        public InputEvent ReloadEvent;

        public void SetDefaults()
        {
            maxAmmo = 30;
            curr_ammo = maxAmmo;
            reloadSpeed = 1f;
            UseAmmoEvent = InputEvent.Character_MainStart;
            ReloadEvent = InputEvent.Usable_Reset;
        }

        public void UseAmmo(int ammoUsed = -1)
        {
            curr_ammo += ammoUsed;
            curr_ammo = Math.Clamp(curr_ammo, 0, maxAmmo);
        }
    }


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
        basicAmmoStats.SetDefaults();
    }
    public AmmoCounter(AmmoCounter copied)
    {
        basicAmmoStats = copied.basicAmmoStats;

    }
    // creates a deepy copy of this class.
    public override StackCounter GetCopy()
    {
        return new AmmoCounter(this);
    }
    public override void SetInputRelay(InputEventRelay newRelay)
    {
        // unsubscribe from the previous relay
        if (inputRelay != null)
        {

        }

        // set new
        inputRelay = newRelay;
        inputRelay.ConnectEvent(UseAmmoEvent, UseAmmo);
    }
    #endregion

    #region Functionality
    public void UseAmmo()
    {
        basicAmmoStats.UseAmmo();
    }
    public void ReloadAmmo(int amt = 0)
    {
        amt = amt == 0 ? maxAmmo : amt; 
    }

    public override void GetEvents(List<InputEvent> inputEventsUsed)
    {
        inputEventsUsed.Add(UseAmmoEvent);
        inputEventsUsed.Add(ReloadEvent);
    }
    #endregion

    #region Stack Status
    // returns a float, which can be used by an array to select a particular index
    public override float GetIndexData()
    {
        return curr_ammo > 0 ? 1f : -1f;
    }
    public override float GetStatus()
    {
        return (float)curr_ammo/maxAmmo;
    }
    #endregion
}
