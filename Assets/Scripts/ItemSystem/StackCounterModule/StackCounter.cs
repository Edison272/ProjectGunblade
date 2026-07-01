using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StackCountType 
{
    Simple, 
    Ammo, 
    Charge, 
    Sequence, 
    Stance
}
/// <summary>
///  A Simple Stack Counter, minimal functionality
/// It's main purpose is to be recast into its more dedicated child classes
/// </summary>

[Serializable]
public class StackCounter
{
    [field: SerializeField] public StackCountType stackCounterType {get; protected set;} = StackCountType.Simple;

    #region Initializers
    public StackCounter()
    {
        
    }
    // creates a deepy copy of this class.
    public virtual StackCounter GetCopy()
    {
        return new StackCounter();
    }
    #endregion

    #region Functionality
    public virtual void ChangeCounter() // 
    {
        
    }

    #endregion




    #region Stack Status
    // Get an "index" which determines how attack types are chosen
    public virtual float GetStackIndex()
    {
        return 0f;
    }
    public virtual bool IsReady()
    {
        return true;
    }

    public virtual float GetStatus()
    {
        return 0.1f;
    }
    #endregion

    #region Helpers
    /// Used to see which events this counter subscribes to.
    /// The array's length should be the amount of InputEvent(enums) there are, so its a true/false to see if it exists or not
    public virtual void GetEvents(bool[] inputEventsUsed)
    {
        return;
    }
    #endregion

    #region Recast Support
    // returns the expecected stack count type enum based on the type of the class
    public StackCountType GetExpectedStackCountType()
    {
        return this switch
        {
            AmmoCounter => StackCountType.Ammo,
            ChargeCounter => StackCountType.Charge,
            SequenceCounter => StackCountType.Sequence,
            _ => StackCountType.Simple
        };
    }
    public StackCounter SmartRecast()
    {
        StackCounter new_type = this;
        switch(stackCounterType)
        {
            case StackCountType.Simple:
                new_type = new StackCounter();
                break;
            case StackCountType.Ammo:
                new_type = new AmmoCounter();
                break;
            case StackCountType.Charge:
                new_type = new AmmoCounter();
                break; 
           case StackCountType.Stance:
                new_type = new AmmoCounter();
                break;
        }
        return new_type;
    }
    #endregion
}