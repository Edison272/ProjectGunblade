using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anything that uses attack objects uses THIS. It contains an array of attack objects. Useful for more attacks with a lot of features
/// AttackObjects can be safetly accessed using 
/// </summary>

[Serializable]
public class AttackObjectController
{
    public float hu = 0;
    [SerializeReference] private AttackObject[] _mainAttack = new AttackObject[] {};
    // connects interaction events to attack objects and other attack objects via mapping
    [SerializeField] private AttackObjectMap[] _attackObjectMap = new AttackObjectMap[] {};
    private readonly Dictionary<AttackObject, int> _attackObjIndexes = new Dictionary<AttackObject, int>();
    public Action GetOnHitEffect;
    public Action GetOnDestroyEffect;

    public void UpdateSerialization()
    {
        for(int i = 0; i < _mainAttack.Length; i++)
        {
            AttackObject attackType = _mainAttack[i];
            if (attackType == null) return;
            if (attackType.UpdateSerialization(out var updated))
                _mainAttack[i] = updated;
            
            _attackObjIndexes[_mainAttack[i]] = i;
        }
    }
    public void StartAttack(int attackIndex, TargetData targData)
    {
        AttackObject atk_obj = GetAttack(attackIndex);
        if (atk_obj != null && attackIndex >= 0 && attackIndex < _attackObjectMap.Length) {
            AttackObjectMap atkMap = _attackObjectMap[attackIndex];
            Debug.Log($"yaineko1 {atkMap.is_subscribed}");
            if (!atkMap.IsSet()) {
                Debug.Log("yaineko");
                AttackEventManager.OnHit += (AttackObject inst_atk_obj) => 
                {
                    if (inst_atk_obj == atk_obj)
                        if (atkMap.OnHitIndex != -1)
                            StartAttack(atkMap.OnHitIndex, targData);
                };
            }
        }
        atk_obj?.Attack(targData);
    }
    public AttackObject GetAttack(int mainAttackIndex)
    {
        if (mainAttackIndex >= 0 && mainAttackIndex < _mainAttack.Length)
            return _mainAttack[mainAttackIndex];
        else
            return null;
    }
    public int GetIndex(AttackObject atkObj)
    {
        if (_attackObjIndexes.TryGetValue(atkObj, out int index))
        {
            return index;
        }
        return -1;
    }
}

/// <summary>
/// A helper class to map an AttackObject to the other AttackObjects it will trigger/create through events
/// </summary>
[Serializable]
public class AttackObjectMap
{
    [Header("Main Object Index")]
    public int attackObjectIndex;
    [Header("Effect Indexes")]
    public int OnHitIndex = -1;
    public bool is_subscribed = false;

    public bool IsSet()
    {
        // only subscribe per type once
        if (!is_subscribed)
        {
            is_subscribed = true;
            return false;
        }
        
            
            
        return true;
    }


}