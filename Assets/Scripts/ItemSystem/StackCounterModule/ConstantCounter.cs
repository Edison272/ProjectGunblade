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
    public InputEvent CallConstant = InputEvent.General_Passive;
    public InputEvent EndConstant = InputEvent.Character_MainEnd;
    // for "forcing" parameters onto some of the input events
    private Action ToggleStart;
    private Action ToggleEnd;
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

        // set force parameter functions
        ToggleStart = () => ToggleConstant(true);
        ToggleEnd = () => ToggleConstant(false);
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
    }
    #endregion

    #region Functionality
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
    public override void GetEvents(List<Enum> inputEventsUsed)
    {
        inputEventsUsed.Add(StartConstant);
        inputEventsUsed.Add(EndConstant);
        inputEventsUsed.Add(CallConstant);
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
