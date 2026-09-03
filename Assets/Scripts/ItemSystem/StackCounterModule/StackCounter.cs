using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;

public enum StackCountType 
{
    Simple, 
    Ammo, 
    Cooldown,
    Charge, 
    Sequence, 
    Stance
}
/// <summary>
///  abstract class for stack counting
/// It's main purpose is to be recast into its more dedicated child classes
/// </summary>

[Serializable]
public abstract class StackCounter
{
    [Header("UI Element")]
    public bool HasUI = true;
    [ShowIf("HasUI")] public StackCounterUISetting UISetting;
    

    [field: SerializeField] public StackCountType stackCounterType {get; protected set;} = StackCountType.Simple;
    protected InputEventRelay inputRelay; // reference to another input relay. used to control when stack interactions happen

    #region Initializers
    public StackCounter() {}
    // creates a deepy copy of this class.
    public abstract StackCounter GetCopy();
    
    #endregion

    #region Functionality
    // Set the input relay/user. unsubscribe from a previous relay if necessary
    public abstract void SetInputRelay(InputEventRelay newRelay);
    #endregion

    #region Stack Status
    // returns a float, which can be used by an array to select a particular index
    public abstract float GetIndexData();
    // provides the precise status data
    public abstract float GetStatus();
    public abstract float GetReadinessTime(); // gets time till readiness
    #endregion


    #region Helpers
    /// Used to see which events this counter subscribes to.
    /// The array's length should be the amount of InputEvent(enums) there are, so its a true/false to see if it exists or not
    public abstract void GetEvents(List<Enum> inputEventsUsed);
    #endregion

    #region Recast Support
    // returns the expecected stack count type enum based on the type of the class
    public StackCountType GetExpectedStackCountType()
    {
        return this switch
        {
            AmmoCounter => StackCountType.Ammo,
            CooldownCounter => StackCountType.Cooldown,
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
                new_type = new SimpleCounter();
                break;
            case StackCountType.Ammo:
                new_type = new AmmoCounter();
                break;
            case StackCountType.Cooldown:
                new_type = new CooldownCounter();
                break;
            case StackCountType.Charge:
                new_type = new ChargeCounter();
                break; 
           case StackCountType.Sequence:
                new_type = new SequenceCounter();
                break;
           case StackCountType.Stance:
                new_type = new AmmoCounter();
                break;
        }
        return new_type;
    }
    #endregion
}

// Basic Starter pack for regions. copy and paste

/*
    #region Initalizers
    #endregion

    #region Base Functionality
    #endregion

    #region Custom Functionality
    #endregion

    #region Stack Status
    #endregion
*/