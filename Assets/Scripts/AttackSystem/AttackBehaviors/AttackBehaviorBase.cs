using UnityEngine;
using AttackSystem;
using GameAI.Factions;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;

/// Shared state and helpers for all spawned attack behaviors (projectile, linecast, melee).
public abstract class AttackBehaviorBase : MonoBehaviour
{
    protected AttackStats atk_stats;
    protected TargetData targetData;
    protected AttackObject atkObject;
    protected Character owner;
    protected int factionTag = FactionManager.NoFactionLayer;
    protected AttackBehaviorBase _poolKey = null;
    public AttackBehaviorBase PoolKey {get {return _poolKey;} set {if (!_poolKey) { _poolKey = value;}}} // used to access the objeect pool in AttackBehaviorPool.cs
    protected const int terrainMask = 1 << 6; // ricochet on contact with these layers
    protected bool attackEnabled = true;
    // Interactions
    public List<AttackObject> OnHitEffects = new List<AttackObject>();
    public List<AttackObject> OnDestroyEffects = new List<AttackObject>();

    /// Call first in every StartX(). Copies stats/target data and resolves owner faction.
    protected void Initialize(AttackObject atkObj, TargetData data)
    {
        SetAttackActive(true);
        atkObject = atkObj;
        atk_stats = atkObj.atk_stats;
        targetData = data;
        owner = data.owner;
        factionTag = owner ? owner.FactionID : FactionManager.NoFactionLayer;

        // set interactions
        if (atkObject.OnHitEffects != null) OnHitEffects.AddRange(atkObject.OnHitEffects);
        if (atkObject.OnDestroyEffects != null) OnDestroyEffects.AddRange(atkObject.OnDestroyEffects);
    }

    /// True if target is a Character on the attacker's faction
    protected bool IsFriendly(GameObject target)
    {
        return target.TryGetComponent<Character>(out Character character)
            && character.FactionID == factionTag;
    }

    /// True if target is the attack's owner
    protected bool IsOwner(GameObject target)
    {
        return owner && target == owner.gameObject;
    }

    protected void ApplyHit(GameObject target, bool activateInteraction = true)
    {
        atk_stats.ApplyData(targetData.sourcePos, target);
        if (activateInteraction)
            ActivateAttackList(OnHitEffects);
    }
    public virtual TargetData GetTarget()
    {
        return targetData.CopyToNewSourcePos(transform.position);
    }

    protected void EndAttack()
    {
        AttackBehaviorPool.RemoveAttack(this);
        ActivateAttackList(OnDestroyEffects);
        SetAttackActive(false);

        // reset any interactions
        OnHitEffects.Clear();
        OnDestroyEffects.Clear();
    }

    public void ActivateAttackList(List<AttackObject> attackList)
    {
        foreach(AttackObject atkObj in attackList)
        {
            atkObj.Attack(GetTarget());
        }
    }

    public virtual void SetAttackActive(bool is_active)
    {
        attackEnabled = is_active;
    }
}