using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ItemStatModules;
using Unity.VisualScripting;

/// <summary>
/// This script has 3 goals
/// - Contain Base Data for the item's various stats
/// - Allow the user to customize items to have different properties
/// - Create new item instances
/// </summary>
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
public class ItemSO : ScriptableObject
{
    [SerializeField] private GameObject item_prefab;
    [field: Header("Equipping")]
    [field: SerializeField] public float EquipTime {get; private set;} = 0.5f;
    [field: SerializeField] public float UnequipTime {get; private set;} = 0.5f;

    [field: Header("Resetting")]
    [field: SerializeField] public float ResetTime {get; private set;} = 1f;

    [field: SerializeReference] public ItemOutput[] ItemOutputs {get; private set;} = new ItemOutput[] {}; // determines the attacks available in this item
    [SerializeReference] public AttackObject[] AttackObjects = new Projectile[] {}; // a collection of attack types
    [SerializeReference] public StackCounter[] StackCounters = new StackCounter[] {}; // control when different item effects trigger

    [field: Header("Aiming")]
    public bool dynamic_aim = true; // allow dynamic aim for the object to be able to turn to face the target
    [Range(0.0f, 1.0f)] public float rotation_scale = 1f;    // 0 to 1

    [field: Header("UI")]
    public Sprite ui_image;





    #region Creating the Item
    public Item GenerateItem(Vector3 pos) // summon an item on the ground
    {
        return GenerateItem(pos, Quaternion.identity);
    }
    public Item GenerateItem(Vector3 pos, Quaternion rotation) // summon an item on the ground
    {
        GameObject item_object = MonoBehaviour.Instantiate(item_prefab, pos, rotation);
        Item new_item = item_object.GetComponent<Item>();
        return new_item;
    }
    public Item GenerateItem(Transform holder) // summon an item on a holder
    {
        GameObject item_object = MonoBehaviour.Instantiate(item_prefab, holder);
        Item new_item = item_object.GetComponent<Item>();
        return new_item;
    }

    // public Item SetupNewItem(Item newItem)
    // {
    //     return
    // }
    #endregion

    public void OnValidate()
    {
        for(int i = 0; i < ItemOutputs.Length; i++)
        {
            ItemOutput itemOutput = ItemOutputs[i];
            if (itemOutput == null)
                itemOutput = new GunOutput();
            if (itemOutput.itemOutputType != itemOutput.GetExpectedItemOutputType())
                itemOutput = itemOutput.SmartRecast();
            ItemOutputs[i] = itemOutput;
        }
        for(int i = 0; i < AttackObjects.Length; i++)
        {
            AttackObject attackType = AttackObjects[i];
            if (attackType == null)
            {
                AttackObjects[i] = new Projectile();
                attackType = AttackObjects[i];
            }
            if (attackType.instance != null)
            {
                if (attackType.GetSpecificAttackObject() != attackType.GetType())
                {
                    AttackObjects[i] = attackType.SmartRecast();
                    attackType = AttackObjects[i];
                }
            }
        }
        for(int i = 0; i < StackCounters.Length; i++)
        {
            StackCounter stackCounter = StackCounters[i];
            if (stackCounter == null)
                stackCounter = new SimpleCounter();
            if (stackCounter.stackCounterType != stackCounter.GetExpectedStackCountType())
                stackCounter = stackCounter.SmartRecast();
            StackCounters[i] = stackCounter;
        }
    }

    #region GUI support

    #endregion
}