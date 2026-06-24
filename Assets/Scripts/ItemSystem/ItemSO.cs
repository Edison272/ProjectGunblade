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
    [SerializeReference] public AttackType[] attackTypes = new AttackType[] {};
    public ItemEffectInit[] itemEffects;

    public void OnValidate()
    {
        for(int i = 0; i < attackTypes.Length; i++)
        {
            AttackType attackType = attackTypes[i];
            if (attackType == null)
            {
                attackTypes[i] = new Projectile();
                attackType = attackTypes[i];
            }
            if (attackType.instance != null)
            {
                if (attackType.GetSpecificAttackType() != attackType.GetType())
                {
                    attackTypes[i] = attackType.SmartRecast();
                    attackType = attackTypes[i];
                }
            }
        }
    }

    #region GUI support

    #endregion
}