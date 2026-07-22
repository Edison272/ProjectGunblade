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
public enum CharacterEvent
{
    MoveStart,
    MoveEnd,
    MainStart, 
    MainEnd, 
    AltStart, 
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