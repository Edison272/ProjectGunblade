using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using ItemStatModules;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Items", order = 1)]
public class ItemSO : ScriptableObject
{
    
    public ItemEffect[] itemEffects = new ItemEffect[] {}; // determines the attacks available in this item
    [SerializeReference] public AttackObject[] attackObjects = new Projectile[] {}; // a collection of attack types
    [SerializeReference] public StackCounter[] stackCounters = new StackCounter[] {}; // control when different item effects trigger

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
                stackCounter = new StackCounter();
            }
            if (stackCounter.stackCounterType != stackCounter.GetExpectedStackCountType())
            {
                stackCounters[i] = stackCounter.SmartRecast();
            }
            
        }
    }

    #region GUI support

    #endregion
}