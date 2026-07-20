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
    public float mass = 10;
    public float speed = 3;
    [Range(0.01f,5f)] public float accel_time = 0.5f;
    public float range = 4;
    public float close_range = 3; // entering close range forces character to retarget
    public float interaction_range = 1;

    public GameObject char_prefab;

    public LayerMask detection_mask;

    [field: Header("Inventory Data")]
    public int holding_capacity = 2;
    public ItemSO[] inventory;
    public ActiveSlot[] inventory_slots;

    public Character GenerateChar(Vector3 pos)
    {
        GameObject op_object = MonoBehaviour.Instantiate(char_prefab, pos, Quaternion.identity);
        Character new_op = op_object.GetComponent<Character>();
        new_op.AssignBaseData(this);

        return new_op;
    }
}
