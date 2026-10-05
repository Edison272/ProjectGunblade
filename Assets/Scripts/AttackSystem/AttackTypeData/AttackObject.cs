using UnityEngine;
using AttackSystem;
using Unity.VisualScripting;

public enum AttackEnum { Projectile, Linecast, MeleeAttack }

/// Serialized attack config. Spawns instances of its prefab and hands each to its behavior script.
/// Subclasses only declare their spread settings and how to launch their behavior.
[System.Serializable]
public abstract class AttackObject
{
    [SerializeField] public GameObject instance; // The outward-facing property to set the instance in code
    [SerializeField, HideInInspector] private AttackBehaviorBase _instance; // the actual instance field which will be set if the instance is accepted
    [SerializeField] public TargetFaction targetFaction;
    [ShowIf("_instance")] public AttackStats atk_stats;
    [ShowIf("_instance")] public AttackTypeData typeData;

    public AttackObject(AttackBehaviorBase instance = null)
    {
        this._instance = instance;
    }
    /// Initialize the spawned instance's behavior.
    /// miss = 0..1, how far a randomly scattered shot strayed from the aim (always 0 for even spread)
    protected abstract void Launch(GameObject spawned, TargetData shot, float miss);

    /// Override to pool instead of instantiate
    protected virtual GameObject Spawn(Vector2 position)
    {
        return AttackBehaviorPool.GetAttack(_instance, position).gameObject;
    }

    public virtual TargetDataRequest GetTargetDataReq()
    {
        TargetDataRequest req = new TargetDataRequest();
        req.TargetFaction = targetFaction;
        return req;
    }

    #region Attack
    public virtual void Attack(TargetData atk_targ)
    {
        Vector2 toTarget = atk_targ.targetPos - atk_targ.sourcePos;
        float baseAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        for (int i = 0; i < typeData.Count; i++)
        {
            Vector2 target = ShotTarget(atk_targ, toTarget.magnitude, baseAngle, i, typeData, out float miss);
            TargetData shot = atk_targ.WithTargetPos(target);
            Launch(Spawn(shot.sourcePos), shot, miss);
        }
    }

    private static Vector2 ShotTarget(TargetData data, float dist, float baseAngle, int index, AttackTypeData atkTypeData, out float miss)
    {
        if (atkTypeData.Even)
        {
            miss = 0f;
            // fan spans the full angle; a single shot goes straight
            float offset = atkTypeData.Count > 1
                ? -atkTypeData.Angle / 2f + atkTypeData.Angle / (atkTypeData.Count - 1) * index
                : 0f;
            float rad = (baseAngle + offset) * Mathf.Deg2Rad;
            return data.sourcePos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * dist;
        }

        Vector2 scatter = Random.insideUnitCircle * (atkTypeData.Angle / 360f); // fraction of dist
        miss = scatter.magnitude;
        return data.targetPos + scatter * dist;
    }
    #endregion

    #region Recasting
    public virtual Projectile RecastToProjectileType() => new Projectile(_instance, atk_stats, new ProjectileTypeData());
    public virtual Linecast RecastToLinecastType() => new Linecast(_instance, atk_stats, new LinecastTypeData());
    public virtual MeleeAttack RecastToMeleeType() => new MeleeAttack(_instance, atk_stats, new MeleeTypeData());
    public virtual AreaEffect RecastToAreaEffectType() => new AreaEffect(_instance, atk_stats, new AreaEffectTypeData());

    public bool UpdateSerialization(out AttackObject result)
    {
        result = this;

        if (instance == null) { _instance = null; return false; }

        // update nested serialization rq
        if(typeData != null) {
            // AttackObject other_atk_obj = null;
            // if (typeData.OnHitAttack.instance) { // AttackObjects are ALWAYS preinitialized
            //     typeData.OnHitAttack.UpdateSerialization(out other_atk_obj);
            //     typeData.OnHitAttack = (Projectile)other_atk_obj;
            // }
            // if (typeData.OnDestroyAttack.instance) {
            //     typeData.OnDestroyAttack.UpdateSerialization(out other_atk_obj);
            //     typeData.OnDestroyAttack = (Projectile)other_atk_obj;
            // }
        }

        if (!instance.TryGetComponent(out AttackBehaviorBase behavior))
        {
            Debug.LogWarning($"'{instance.name}' has no AttackBehaviorBase. Rejected.");
            instance = null;
            _instance = null;
            return false;
        }

        _instance = behavior; // also resyncs after domain reload

        AttackObject recast = behavior switch
        {
            ProjectileBehavior when this is not Projectile => RecastToProjectileType(),
            LinecastBehavior   when this is not Linecast   => RecastToLinecastType(),
            MeleeBehavior      when this is not MeleeAttack => RecastToMeleeType(),
            AreaEffectBehavior when this is not AreaEffect => RecastToAreaEffectType(),
            _ => null
        };

        if (recast == null) return false;

        recast.instance = instance; // recast constructors only set _instance
        result = recast;
        return true;
    }
    #endregion

    public virtual bool Hasinstance() => _instance;
}

#region Projectile
[System.Serializable]
public class Projectile : AttackObject
{
    [ShowIf("_instance")] public new ProjectileTypeData typeData;

    public Projectile(AttackBehaviorBase instance = null) : base(instance) { }
    public Projectile(AttackBehaviorBase instance, AttackStats atk_stats, ProjectileTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    public override TargetDataRequest GetTargetDataReq()
    {
        TargetDataRequest req = base.GetTargetDataReq();
        req.HomingRadius = typeData.homing_radius;
        return req;
    }

    // scattered shots lose speed the further they miss (clamped to 50%)
    protected override void Launch(GameObject spawned, TargetData shot, float miss) =>
        spawned.GetComponent<ProjectileBehavior>().StartProjectile(this, shot, Mathf.Clamp(1f - miss, 0.5f, 1f));
}
#endregion
#region Linecast

[System.Serializable]
public class Linecast : AttackObject
{
    [ShowIf("_instance")] public new LinecastTypeData typeData;

    public Linecast(AttackBehaviorBase instance = null) : base(instance) { }
    public Linecast(AttackBehaviorBase instance, AttackStats atk_stats, LinecastTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override void Launch(GameObject spawned, TargetData shot, float miss) =>
        spawned.GetComponent<LinecastBehavior>().StartLinecast(this, shot);
}
#endregion
#region MeleeAttack
[System.Serializable]
public class MeleeAttack : AttackObject
{
    [ShowIf("_instance")] public new MeleeTypeData typeData;

    public MeleeAttack(AttackBehaviorBase instance = null) : base(instance) { }
    public MeleeAttack(AttackBehaviorBase instance, AttackStats atk_stats, MeleeTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override void Launch(GameObject spawned, TargetData shot, float miss) =>
        spawned.GetComponent<MeleeBehavior>().StartMelee(this, shot);
}
#endregion
#region AreaEffect
[System.Serializable]
public class AreaEffect : AttackObject
{
    [ShowIf("_instance")] public new AreaEffectTypeData typeData;

    public AreaEffect(AttackBehaviorBase instance = null) : base(instance) { }
    public AreaEffect(AttackBehaviorBase instance, AttackStats atk_stats, AreaEffectTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override void Launch(GameObject spawned, TargetData shot, float miss) =>
        spawned.GetComponent<AreaEffectBehavior>().StartAreaEffect(this, shot);
}
#endregion
