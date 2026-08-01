using System;
using UnityEngine;

/*
WHEN ADDING NEW EVENTS, MAKE SURE TO UPDATE:
- InputEventSelector.cs
- EventTypeContainer.cs

WITH THE NEW EVENTS! THIS IS CRUCIAL FOR MAKING TS WORK!!!
*/

public enum GlobalEvent
{
    Update, 
}
public enum InterfaceEvent
{
    Scroll,
}
public enum CharacterEvent
{
    MoveStart,
    MoveEnd,
    
    // Main is generally LMB
    MainStart, 
    MainUpdate,
    MainEnd, 

    // Alt is same as main but RMB
    AltStart, 
    AltUpdate,
    AltEnd,

    LookPos,
    Interact,
    InventorySelect,
}

public enum UsableEvent
{
    Equip,
    Unequip,

    Used,
    ResetStart, 
    ResetEnd, 
}