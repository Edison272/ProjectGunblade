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
    public InputEventSelector StartConstant = new InputEventSelector();
    public InputEventSelector CallConstant = new InputEventSelector(); // this is to be unchanged. the whole point of this counter is to passive
    public InputEventSelector EndConstant = new InputEventSelector();
    public InputEventSelector ResetConstant = new InputEventSelector();
    // for "forcing" parameters onto some of the input events
    private Delegate ToggleStart;
    private Delegate ToggleEnd;
    private bool _constantActive = false;

    #region Initalizers
    public ConstantCounter()
    {
        stackCounterType = GetExpectedStackCountType();
        StartConstant.SetInputEvent(CharacterEvent.MainStart);
        CallConstant.SetInputEvent(GlobalEvent.Update);
        EndConstant.SetInputEvent(CharacterEvent.MainEnd);
        ResetConstant.SetInputEvent(UsableEvent.ResetStart);
    }
    public ConstantCounter(ConstantCounter copied)
    {
        StartConstant = copied.StartConstant;
        EndConstant = copied.EndConstant;
        ResetConstant.SetInputEvent(UsableEvent.ResetStart);

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
        inputRelay.ConnectEvent(StartConstant.InputEvent, ToggleStart);
        inputRelay.ConnectEvent(EndConstant.InputEvent, ToggleEnd);
        inputRelay.ConnectEvent(CallConstant.InputEvent, UpdateConstant);
        inputRelay.ConnectEvent(ResetConstant.InputEvent, ResetCounter);
    }
    #endregion

    #region Base Functionality
    public override void GetEvents(List<Enum> inputEventsUsed)
    {
        inputEventsUsed.Add(StartConstant.InputEvent);
        inputEventsUsed.Add(EndConstant.InputEvent);
        inputEventsUsed.Add(CallConstant.InputEvent);
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
        Debug.Log("Aka");
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
