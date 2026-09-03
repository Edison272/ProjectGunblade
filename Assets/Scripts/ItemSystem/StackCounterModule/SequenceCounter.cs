using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Sequence counter is effectively an iterator, starting at 0 and resetting after reaching a maximum
/// Most commonly used for items with combo patterns, for which it will launch different attacks depending on the sequence
/// - Accumulates stacks as ints, so good for discrete events
/// - Resets automatically after reaching the MaxIndex
/// </summary>
[Serializable]
public class SequenceCounter : StackCounter
{
    private int _currIndex = 0;
    public int MaxIndex = 0;
    public InputEventSelector IterateSequenceEvent = new InputEventSelector(UsableEvent.Used);
    public InputEventSelector ResetSequenceEvent = new InputEventSelector(UsableEvent.ResetStart);
    
    public bool ResetAfterCooldown = false;
    [ShowIf("ResetAfterCooldown")] public float SequenceResetTime = 1f;
    
    #region Initalizers
    public SequenceCounter()
    {
        stackCounterType = GetExpectedStackCountType();
    }
    public SequenceCounter(SequenceCounter copied)
    {
        _currIndex = 0;
        MaxIndex = copied.MaxIndex;
        IterateSequenceEvent = copied.IterateSequenceEvent;
        ResetSequenceEvent = copied.ResetSequenceEvent;
        UISetting = copied.UISetting;
        HasUI = copied.HasUI;
    }
    public override StackCounter GetCopy()
    {
        return new SequenceCounter(this);
    }
    public override void SetInputRelay(InputEventRelay newRelay)
    {
        // unsubscribe from the previous relay
        if (inputRelay != null)
        {

        }

        // set new
        inputRelay = newRelay;
        inputRelay.ConnectEvent(IterateSequenceEvent.InputEvent, IterateSequence);
        inputRelay.ConnectEvent(ResetSequenceEvent.InputEvent, ResetCounter);
    }
    #endregion

    #region Base Functionality
    public override void GetEvents(List<Enum> inputEventsUsed)
    {
        inputEventsUsed.Add(IterateSequenceEvent.InputEvent);
        inputEventsUsed.Add(ResetSequenceEvent.InputEvent);
    }

    #endregion

    #region Custom Functionality
    public virtual void ResetCounter()
    {
        _currIndex = 0;
    }
    public virtual void IterateSequence()
    {
        _currIndex++;
        if (_currIndex > MaxIndex)
        {
            _currIndex = 0;
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
        return ((float)_currIndex) / MaxIndex;
    }
    public override float GetReadinessTime()
    {
        return Mathf.Infinity; // no time sensitive values, so not relevant for readiness
    }
    #endregion
    
    #region RawData
    public override float GetMaxValue()
    {
        throw new NotImplementedException();
    }

    public override float GetCurrentValue()
    {
        throw new NotImplementedException();
    }
    #endregion
}