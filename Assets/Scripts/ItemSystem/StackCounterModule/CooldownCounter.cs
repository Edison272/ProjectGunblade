 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Repeater Counters release an output at a fixed rate when inputs are activated
/// This is effectively the definitive "attack speed" counter for all semi and full auto needs
/// </summary>
[Serializable]
public class CooldownCounter : StackCounter
{
    [Header("Basic Stats")]
    public float cooldownTime = 0.1f;
    private float lastUse = 0;
    public InputEvent StartCooldownEvent = InputEvent.Character_MainStart;
    public InputEvent ResetCooldownEvent = InputEvent.Usable_ResetStart;

    #region Initalizers
    public CooldownCounter()
    {
        stackCounterType = StackCountType.Cooldown;
    }
    public CooldownCounter(CooldownCounter copied)
    {
        cooldownTime = copied.cooldownTime;
        StartCooldownEvent = copied.StartCooldownEvent;
    }
    // creates a deepy copy of this class.
    public override StackCounter GetCopy()
    {
        return new CooldownCounter(this);
    }
    public override void SetInputRelay(InputEventRelay newRelay)
    {
        // unsubscribe from the previous relay
        if (inputRelay != null)
        {

        }

        // set new
        inputRelay = newRelay;
        inputRelay.ConnectEvent(StartCooldownEvent, StartCooldown);
        inputRelay.ConnectEvent(ResetCooldownEvent, ResetCounter);
    }
    #endregion
    #region Base Functionality
    public virtual void ResetCounter()
    {
        lastUse = Time.time;
    }
    public override void GetEvents(List<InputEvent> inputEventsUsed)
    {
        inputEventsUsed.Add(StartCooldownEvent);
        inputEventsUsed.Add(ResetCooldownEvent);
    }
    #endregion

    #region Functionality
    public void StartCooldown()
    {
        if (GetIndexData() > 0)
        {
            UseActivator();
            lastUse = Time.time + cooldownTime;
        }
    }

    #endregion
    
    #region Stack Status
    // returns a float, which can be used by an array to select a particular index
    public override float GetIndexData()
    {
        return Time.time >= lastUse ? 1f : -1;
    }
    public override float GetStatus()
    {
        return Mathf.Clamp01(Time.time / lastUse);
    }
    #endregion




}
