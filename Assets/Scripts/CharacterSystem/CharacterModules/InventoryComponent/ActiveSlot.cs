using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  a "slot" in the player's inventory. Holds references to one or multiple indexes of an inventory array
/// </summary>
[System.Serializable]
public class ActiveSlot
{
    public int MainSlot = 0;
    public int AltSlot = -1;
    public bool IsAkimbo => AltSlot > 0 ? true : false;
    public ActiveSlot(ActiveSlot copied)
    {
        MainSlot = copied.MainSlot;
        AltSlot = copied.AltSlot;
    }
    public void SetSlotAcive(List<Item> inventory, bool is_active)
    {
        inventory[MainSlot].SetEquipped(is_active);
        if (AltSlot > -1)
        {
            inventory[AltSlot].SetEquipped(is_active);
        }
    }
}