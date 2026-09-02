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
    [field: SerializeField] public int MainSlot {get; private set;} = -1;
    [field: SerializeField] public int AltSlot {get; private set;} = -1;
    private Item[] _inventoryRef;
    public bool IsAkimbo => AltSlot > 0 ? true : false;
    public float EquipSpeed => GetEquipSpeed(false);
    public float UnequipSpeed => GetEquipSpeed(true);
    public bool IsEmpty => MainSlot == -1 && AltSlot == -1;

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
    #region Getting Data
    // returns a score for how usable or "ready" the items in this slot are
    // average the scores if both slots are active
    public float GetSlotReadiness()
    {
        float readinessScore = 0;
        float dividend = 1;
        if (MainSlot > -1)
            readinessScore += _inventoryRef[MainSlot].ReadinessScore;
        if (AltSlot > -1)
            readinessScore += _inventoryRef[AltSlot].ReadinessScore;
            dividend += 1;

        return readinessScore/dividend;
    }
    // a simple function to see if anything needs a reset
    public bool GetSlotNeedsReset()
    {
        bool needsReset = false;
        if (MainSlot > -1)
            needsReset = needsReset || _inventoryRef[MainSlot].NeedsReset;
        if (AltSlot > -1)
            needsReset = needsReset || _inventoryRef[AltSlot].NeedsReset;

        return needsReset;
    }
    #endregion
    
    #region Setting Indexes
    public void SetIndexes(int MainSlot, int AltSlot)
    {
        SetMainIndex(MainSlot);
        SetAltIndex(AltSlot);
    }
    public void SetMainIndex(int MainSlot)
    {
        if (MainSlot > -1 && MainSlot < _inventoryRef.Length - 1)
            this.MainSlot = MainSlot;
    }

    public void SetAltIndex(int AltSlot)
    {
        if (AltSlot > -1 && AltSlot < _inventoryRef.Length - 1)
            this.AltSlot = AltSlot;
    }
    #endregion

    #region Helpers
    // true for getting unequip speed, false for getting equip speed
    private float GetEquipSpeed(bool is_unequip)
    {
        float speed = 0;
        if (MainSlot > -1)
            speed += is_unequip ? _inventoryRef[MainSlot].UnequipTime : _inventoryRef[MainSlot].EquipTime;
        if (AltSlot > -1)
            speed += is_unequip ? _inventoryRef[AltSlot].UnequipTime : _inventoryRef[AltSlot].EquipTime;
        Debug.Log($"Changing Item in {speed}");
        return speed;
    }
    #endregion
}