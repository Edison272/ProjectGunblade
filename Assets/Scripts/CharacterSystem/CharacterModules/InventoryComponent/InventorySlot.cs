using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  a "slot" in the player's inventory. Holds references to one or multiple indexes of an inventory array
/// </summary>
[System.Serializable]
public class InventorySlot
{
    public int MainSlot = -1;
    public int AltSlot = -1;
    private Item[] _inventoryRef;
    public bool IsAkimbo => AltSlot > 0 ? true : false;
    public float EquipSpeed => GetEquipSpeed(false);
    public float UnequipSpeed => GetEquipSpeed(true);

    public InventorySlot()
    {
        MainSlot = -1;
        AltSlot = -1;
    }
    public InventorySlot(int MainSlotIndex, int AltSlotIndex = -1)
    {
        MainSlot = MainSlotIndex;
        AltSlot = AltSlotIndex;
    }

    public InventorySlot(InventorySlot copied)
    {
        MainSlot = copied.MainSlot;
        AltSlot = copied.AltSlot;
        _inventoryRef = copied._inventoryRef;
    }

    public InventorySlot SetInventoryReference(Item[] setInentory)
    {
        _inventoryRef = setInentory;
        return this;
    }
    public void SetSlotAcive(bool is_active)
    {
        if (MainSlot > -1)
            _inventoryRef[MainSlot].SetEquipped(is_active);
        if (AltSlot > -1)
            _inventoryRef[AltSlot].SetEquipped(is_active);

    }

    #region Helpers
    // true for getting unequip speed, false for getting equip speed
    private float GetEquipSpeed(bool is_unequip)
    {
        float speed = 0;
        if (MainSlot > -1)
            speed += is_unequip ? _inventoryRef[MainSlot].UnequipTime : _inventoryRef[MainSlot].EquipTime;
        if (AltSlot > -1)
            speed += is_unequip ? _inventoryRef[AltSlot].UnequipTime : _inventoryRef[AltSlot].EquipTime;
        return speed;
    }

    public bool ContainsItem()
    {
        return MainSlot > -1 && AltSlot > -1;
    }
    #endregion
}