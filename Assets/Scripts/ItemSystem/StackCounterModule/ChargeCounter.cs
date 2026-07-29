using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The charge counter accumulates stacks, before releasing them
/// - Methods to start/stop charging
/// - Accumulates counters based on time, AKA uses a float value for stacks
/// - Call activator effect when charging stops
/// </summary>
[Serializable]
public class ChargeCounter : StackCounter
{
    public float MaxChargeTime = 1;
    public float MinChargeTime = 0;
    private float _startChargeTime;
    public InputEventSelector StartChargeEvent = new InputEventSelector(CharacterEvent.MainStart);
    public InputEventSelector StopChargeEvent = new InputEventSelector(CharacterEvent.MainEnd);
    public InputEventSelector ResetChargeEvent = new InputEventSelector(UsableEvent.ResetStart);
    
    #region Initalizers
    public ChargeCounter()
    {
        stackCounterType = GetExpectedStackCountType();
    }
    public ChargeCounter(ChargeCounter copied)
    {
        MaxChargeTime = copied.MaxChargeTime;
        MinChargeTime = copied.MinChargeTime;
        StartChargeEvent = copied.StartChargeEvent;
        StopChargeEvent = copied.StopChargeEvent;
        ResetChargeEvent = copied.ResetChargeEvent;
    }
    public override StackCounter GetCopy()
    {
        return new ChargeCounter(this);
    }
    public override void SetInputRelay(InputEventRelay newRelay)
    {
        // unsubscribe from the previous relay
        if (inputRelay != null)
        {

        }

        // set new
        inputRelay = newRelay;
        inputRelay.ConnectEvent(StartChargeEvent.InputEvent, StartCharging);
        inputRelay.ConnectEvent(StopChargeEvent.InputEvent, StopCharging);
        inputRelay.ConnectEvent(ResetChargeEvent.InputEvent, ResetCounter);
    }
    #endregion

    #region Base Functionality
    public override void GetEvents(List<Enum> inputEventsUsed)
    {
        inputEventsUsed.Add(StartChargeEvent.InputEvent);
        inputEventsUsed.Add(StopChargeEvent.InputEvent);
        inputEventsUsed.Add(ResetChargeEvent.InputEvent);
    }
    public virtual void ResetCounter()
    {
        _startChargeTime = 0;
    }
    #endregion

    #region Custom Functionality
    private void StartCharging()
    {
        _startChargeTime = Time.time;
    }
    private void StopCharging()
    {
        if (Time.time - _startChargeTime > MinChargeTime)
        {
            UseActivator();
            _startChargeTime = 0;
        }
    }

    #endregion

    #region Stack Status
    public override float GetIndexData()
    {
        return GetStatus();
    }

    public override float GetStatus()
    {
        float currChargeTime = (Time.time - _startChargeTime - MinChargeTime);
        return Mathf.Clamp01(currChargeTime / (MaxChargeTime));
    }
    #endregion
}