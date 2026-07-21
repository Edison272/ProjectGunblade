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
    [field: SerializeField] public float resetTime {get; private set;} = 1f;
    public ItemEffect[] itemEffects = new ItemEffect[] {}; // determines the attacks available in this item
    [SerializeReference] public AttackObject[] attackObjects = new Projectile[] {}; // a collection of attack types
    [SerializeReference] public StackCounter[] stackCounters = new StackCounter[] {}; // control when different item effects trigger

    [field: Header("Aiming")]
    public bool dynamic_aim = true; // allow dynamic aim for the object to be able to turn to face the target
    [Range(0.0f, 1.0f)] public float rotation_scale = 1f;    // 0 to 1

    [field: Header("Equipping")]
    [field: SerializeField] public float EquipTime {get; private set;} = 0.5f;
    [field: SerializeField] public float UnequipTime {get; private set;} = 0.5f;

    #region Creating the Item
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
        for(int i = 0; i < attackObjects.Length; i++)
        {
            AttackObject attackType = attackObjects[i];
            if (attackType == null)
            {
                attackObjects[i] = new Projectile();
                attackType = attackObjects[i];
            }
            if (attackType.instance != null)
            {
                if (attackType.GetSpecificAttackObject() != attackType.GetType())
                {
                    attackObjects[i] = attackType.SmartRecast();
                    attackType = attackObjects[i];
                }
            }
        }
        for(int i = 0; i < stackCounters.Length; i++)
        {
            StackCounter stackCounter = stackCounters[i];
            if (stackCounter == null)
            {
                stackCounter = new SimpleCounter();
            }
            if (stackCounter.stackCounterType != stackCounter.GetExpectedStackCountType())
            {
                stackCounter = stackCounter.SmartRecast();
            }

            stackCounters[i] = stackCounter;
            
        }
    }

    #region GUI support

    #endregion
}