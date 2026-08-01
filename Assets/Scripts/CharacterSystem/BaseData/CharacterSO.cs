using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Entities/Character", order = 1)]
public class CharacterSO : ScriptableObject
{
    //id
    public string character_name = "Entity";
    public Sprite character_image;
    public string description = "bleh";

    //stats
    [field: Header("Health")]
    public int health = 100;
    public int spawn_shield = 0;

    [field: Header("Movement")]
    public float weight = 10;
    public float speed = 3;




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
}
