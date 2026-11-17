using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Entities/Character", order = 1)]
public class CharacterSO : ScriptableObject
{
    //id
    public string CharacterName = "Entity";
    public Sprite character_image;
    public string description = "bleh";

    //stats
    [field: Header("Health Data")]
    public HealthComponent Health;

    [field: Header("Movement Data")]
    public MovementComponent Movement;

    [field: Header("Aiming")]
    public float AimStrength;



    public float range = 4;
    public float close_range = 3; // entering close range forces character to retarget
    public float interaction_range = 1;

    public GameObject char_prefab;

    public LayerMask detection_mask;

    [field: Header("Inventory Data")]
    public int HoldingCapacity = 8;
    public int TotalItemSlots = 2;
    public ItemSO[] inventory;
    public InventorySlot[] inventory_slots;

    [field: Header("AI Data")]
    public float DetectionRange;
    public Character Spawn(Vector2 spawnPos, int setFactionID)
    {
        if (char_prefab.TryGetComponent<Character>(out Character base_char))
        {
            return base_char.NewCharacter(spawnPos, setFactionID, this);
        }
        return null;
    }
}
