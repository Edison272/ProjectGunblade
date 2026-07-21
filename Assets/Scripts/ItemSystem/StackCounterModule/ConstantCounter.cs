 using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Constant counter will constantly call the activation function
/// Only when the constant counter is in a "started" state will it then allow for another thing to be called.
/// </summary>
[Serializable]
public class ConstantCounter : StackCounter
{
    [Header("Basic Stats")]
    public InputEvent StartConstant = InputEvent.Character_MainStart;
    public InputEvent CallConstant = InputEvent.General_Passive; // this is to be unchanged. the whole point of this counter is to passive
    public InputEvent EndConstant = InputEvent.Character_MainEnd;
    public InputEvent ResetConstant = InputEvent.Usable_ResetStart;
    // for "forcing" parameters onto some of the input events
    private Delegate ToggleStart;
    private Delegate ToggleEnd;
    private bool _constantActive = false;

    #region Initalizers
    public ConstantCounter()
    {
        stackCounterType = GetExpectedStackCountType();


    }
    public ConstantCounter(ConstantCounter copied)
    {
        StartConstant = copied.StartConstant;
        EndConstant = copied.EndConstant;
        ResetConstant = InputEvent.Usable_ResetStart;

        // set force parameter functions
        ToggleStart = (Action)(() => ToggleConstant(true));
        ToggleEnd = (Action)(() => ToggleConstant(false));
    }
    // creates a deepy copy of this class.
    public override StackCounter GetCopy()
    {
        return new ConstantCounter(this);
    }
    public override void SetInputRelay(InputEventRelay newRelay)
    {
        // unsubscribe from the previous relay
        if (inputRelay != null)
        {
            
        }
        
        // set new
        inputRelay = newRelay;
        inputRelay.ConnectEvent(StartConstant, ToggleStart);
        inputRelay.ConnectEvent(EndConstant, ToggleEnd);
        inputRelay.ConnectEvent(CallConstant, UpdateConstant);
        inputRelay.ConnectEvent(ResetConstant, ResetCounter);
    }
    #endregion

    #region Base Functionality
    public override void GetEvents(List<InputEvent> inputEventsUsed)
    {
        inputEventsUsed.Add(StartConstant);
        inputEventsUsed.Add(EndConstant);
        inputEventsUsed.Add(CallConstant);
    }
    public virtual void ResetCounter()
    {
        ToggleConstant(false);
    }
    #endregion

    #region Custom Functionality
    public void ToggleConstant(bool isActive)
    {
        _constantActive = isActive;
    }
    public void UpdateConstant()
    {
        if (_constantActive)
        {
            UseActivator();
        }
    }
    #endregion

    #region Stack Status
    // returns a float, which can be used by an array to select a particular index
    public override float GetIndexData()
    {
        return _constantActive ? 1f : -1;
    }
    public override float GetStatus()
    {
        return GetIndexData();
    }
    #endregion
}
