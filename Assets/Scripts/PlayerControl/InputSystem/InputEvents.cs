using System;
using UnityEngine;
public enum InputEvent {
    Character_MoveStart,
    Character_MoveEnd,
    Character_MainStart, 
    Character_MainEnd, 
    Character_AltStart, 
    Character_AltEnd,
    Character_LookPos,
    General_Passive, 
    General_StackActivation,
    Usable_Equip,
    Usable_Unequip,
    Usable_Used, 
    Usable_ResetStart, 
    Usable_ResetEnd, 
    Character_Interact,
    Character_InventorySelect,
    Size
}