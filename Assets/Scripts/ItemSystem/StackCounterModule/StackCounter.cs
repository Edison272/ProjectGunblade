using System;
using System.Collections;
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
    #region Functionality
    public virtual void ChangeCounter() // 
    {
        
    }

    #endregion

    #region Stack Status
    public virtual bool IsReady()
    {
        return true;
    }

    public virtual float GetStatus()
    {
        return 0.1f;
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