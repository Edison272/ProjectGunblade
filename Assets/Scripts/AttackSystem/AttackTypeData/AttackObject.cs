using UnityEngine;
using AttackSystem;

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

    public AttackObject(AttackBehaviorBase instance = null)
    {
        this._instance = instance;
    }

    #region Subclass Hooks
    protected readonly struct SpreadSettings
    {
        public readonly int Count;
        public readonly float Angle; // degrees
        public readonly bool Even;   // true = fan evenly across Angle, false = random scatter

        public SpreadSettings(int count, float angle, bool even)
        {
            Count = count;
            Angle = angle;
            Even = even;
        }
    }

    protected abstract SpreadSettings Spread { get; }

    /// Initialize the spawned instance's behavior.
    /// miss = 0..1, how far a randomly scattered shot strayed from the aim (always 0 for even spread)
    protected abstract void Launch(GameObject spawned, TargetData shot, float miss);

    /// Override to pool instead of instantiate
    protected virtual GameObject Spawn(Vector2 position)
    {
        return AttackBehaviorPool.GetAttack(_instance, position).gameObject;
    }
    #endregion

    public float GetAtkSpread() => Spread.Angle;

    public virtual TargetDataRequest GetTargetDataReq()
    {
        TargetDataRequest req = new TargetDataRequest();
        req.TargetFaction = targetFaction;
        return req;
    }

    #region Attack
    public virtual void Attack(TargetData atk_targ)
    {
        SpreadSettings spread = Spread;
        Vector2 toTarget = atk_targ.targetPos - atk_targ.sourcePos;
        float baseAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        for (int i = 0; i < spread.Count; i++)
        {
            Vector2 target = ShotTarget(atk_targ, toTarget.magnitude, baseAngle, i, spread, out float miss);
            TargetData shot = atk_targ.WithTargetPos(target);
            Launch(Spawn(shot.sourcePos), shot, miss);
        }
    }

    private static Vector2 ShotTarget(TargetData data, float dist, float baseAngle, int index, SpreadSettings spread, out float miss)
    {
        if (spread.Even)
        {
            miss = 0f;
            // fan spans the full angle; a single shot goes straight
            float offset = spread.Count > 1
                ? -spread.Angle / 2f + spread.Angle / (spread.Count - 1) * index
                : 0f;
            float rad = (baseAngle + offset) * Mathf.Deg2Rad;
            return data.sourcePos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * dist;
        }

        Vector2 scatter = Random.insideUnitCircle * (spread.Angle / 360f); // fraction of dist
        miss = scatter.magnitude;
        return data.targetPos + scatter * dist;
    }
    #endregion

    #region Recasting
    public virtual Projectile RecastToProjectileType() => new Projectile(_instance, atk_stats, new ProjectileTypeData());
    public virtual Linecast RecastToLinecastType() => new Linecast(_instance, atk_stats, new LinecastTypeData());
    public virtual MeleeAttack RecastToMeleeType() => new MeleeAttack(_instance, atk_stats, new MeleeTypeData());

    public bool UpdateSerialization(out AttackObject result)
    {
        result = this;

        if (instance == null) { _instance = null; return false; }

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
    [ShowIf("_instance")] public ProjectileTypeData typeData;

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

    protected override SpreadSettings Spread =>
        new SpreadSettings(typeData.projectile_count, typeData.projectile_spread, typeData.even_spread);

    // scattered shots lose speed the further they miss (clamped to 50%)
    protected override void Launch(GameObject spawned, TargetData shot, float miss) =>
        spawned.GetComponent<ProjectileBehavior>().StartProjectile(this, shot, Mathf.Clamp(1f - miss, 0.5f, 1f));
}
#endregion
#region Linecast

[System.Serializable]
public class Linecast : AttackObject
{
    [ShowIf("_instance")] public LinecastTypeData typeData;

    public Linecast(AttackBehaviorBase instance = null) : base(instance) { }
    public Linecast(AttackBehaviorBase instance, AttackStats atk_stats, LinecastTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override SpreadSettings Spread =>
        new SpreadSettings(typeData.linecast_count, typeData.linecast_spread, typeData.even_spread);

    protected override void Launch(GameObject spawned, TargetData shot, float miss) =>
        spawned.GetComponent<LinecastBehavior>().StartLinecast(this, shot);
}
#endregion
#region MeleeAttack
[System.Serializable]
public class MeleeAttack : AttackObject
{
    [ShowIf("_instance")] public MeleeTypeData typeData;

    public MeleeAttack(AttackBehaviorBase instance = null) : base(instance) { }
    public MeleeAttack(AttackBehaviorBase instance, AttackStats atk_stats, MeleeTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override SpreadSettings Spread =>
        new SpreadSettings(typeData.melee_count, typeData.melee_spread, typeData.even_spread);

    protected override void Launch(GameObject spawned, TargetData shot, float miss) =>
        spawned.GetComponent<MeleeBehavior>().StartMelee(this, shot);
}
#endregion
