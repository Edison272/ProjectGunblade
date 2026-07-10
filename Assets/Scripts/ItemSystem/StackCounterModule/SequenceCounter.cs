using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The simple counter is a simple counter. yes.
/// It is the default counter when the value is initialized if OnValidate is active.
/// All this counter does is keep track of one counter value and one default value.
/// </summary>
public class SequenceCounter : StackCounter
{
    public int curr_stacks;
    public int default_stacks;
    
    #region Initalizers
    public SequenceCounter() {}
    public SequenceCounter(SequenceCounter copied)
    {
        default_stacks = copied.default_stacks;
        curr_stacks = default_stacks;
    }
    public override StackCounter GetCopy()
    {
        return new SequenceCounter(this);
    }
    public override void SetInputRelay(InputEventRelay newRelay)
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Base Functionality
    public override void GetEvents(List<InputEvent> inputEventsUsed)
    {
        throw new NotImplementedException();
    }
    public virtual void ResetCounter()
    {
        throw new NotImplementedException();
    }
    #endregion

    #region Custom Functionality

    #endregion

    #region Stack Status
    public override float GetIndexData()
    {
        throw new NotImplementedException();
    }

    public override float GetStatus()
    {
        throw new NotImplementedException();
    }
    #endregion
}