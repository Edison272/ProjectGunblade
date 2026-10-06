using System;
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
    [SerializeReference] private AttackObject[] _onHitAttack = new AttackObject[] {};

    public void UpdateSerialization()
    {
        for(int i = 0; i < _mainAttack.Length; i++)
        {
            AttackObject attackType = _mainAttack[i];
            if (attackType == null) return;
            if (attackType.UpdateSerialization(out var updated))
                _mainAttack[i] = updated;
        }
    }
    public void StartAttack(int attackIndex, TargetData targData)
    {
        AttackObject atk_obj = GetAttack(attackIndex);
        atk_obj?.Attack(targData);
    }
    public AttackObject GetAttack(int mainAttackIndex)
    {
        if (mainAttackIndex >= 0 && mainAttackIndex < _mainAttack.Length)
            return _mainAttack[mainAttackIndex];
        else
            return null;
    }
}