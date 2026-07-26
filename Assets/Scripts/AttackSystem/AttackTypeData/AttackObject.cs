using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AttackSystem;

using Random = UnityEngine.Random;
using JetBrains.Annotations;

public enum AttackEnum { Projectile, Linecast, MeleeAttack }
[Serializable]
public abstract class AttackObject
{
    protected Character user;
    [SerializeField] public GameObject instance;
    [ShowIf("instance")] public AttackStats atk_stats;
    #region Initializers
    public AttackObject(GameObject instance = null)
    {
        this.instance = instance;
    }
    public virtual TargetDataRequest GetTargetDataReq()
    {
        TargetDataRequest targetDataRequest = new TargetDataRequest();
        return targetDataRequest;
    }
    #endregion
    public abstract void Attack(TargetData atk_targ);
    public abstract float GetAtkSpread();

    #region Recasting
    /// Basic AttackObject can be recast into its inheritor scripts
    public virtual Projectile RecastToProjectileType()
    {
        return new Projectile(instance, atk_stats, new ProjectileTypeData());
    }
    public virtual Linecast RecastToLinecastType()
    {
        return new Linecast(instance, atk_stats, new ProjectileTypeData());
    }
    public virtual MeleeAttack RecastToMeleeType()
    {
        return new MeleeAttack(instance, atk_stats, new MeleeTypeData());
    }
    // Will automatically recast its type based on the instance it's holding
    public virtual AttackObject SmartRecast()
    {
        AttackObject new_type = null;
        switch(instance.GetComponent<MonoBehaviour>())
        {
            case ProjectileBehavior:
                new_type = RecastToProjectileType();
                break;
            case LinecastBehavior:
                new_type = RecastToLinecastType();
                break; 
           case MeleeBehavior:
                new_type = RecastToMeleeType();
                break;
        }
        return new_type;
    }

    /// Returns either Projectile, Linecast, or MeleeAttack, but in the form of a AttackObject
    /// Used by ItemSO to check if the attack's instance type matches with the attack data
    public virtual Type GetSpecificAttackObject()
    {
        if (instance == null)
        {
            return typeof(AttackObject);
        }
        switch(instance.GetComponent<MonoBehaviour>())
        {
            case ProjectileBehavior:
                return typeof(Projectile);
            case LinecastBehavior:
                return typeof(Linecast);
            case MeleeBehavior:
                return typeof(MeleeAttack);
            default:
                return typeof(Projectile);
        }
    }
    #endregion

    public virtual bool Hasinstance()
    {
        return instance;
    }

}
#region Projectile
[System.Serializable]
public class Projectile : AttackObject
{
    [ShowIf("instance")] public ProjectileTypeData typeData;
    
    #region Initializers
    public Projectile(GameObject instance = null) : base(instance) {}
    public Projectile(GameObject instance, AttackStats atk_stats, ProjectileTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }
    public override TargetDataRequest GetTargetDataReq()
    {
        TargetDataRequest targetDataRequest = new TargetDataRequest();
        targetDataRequest.HomingRadius = typeData.homing_radius;
        return targetDataRequest;
    }
    #endregion

    public override void Attack(TargetData atk_targ)
    {
        Vector2 targetPos_og = atk_targ.targetPos;
        Vector2 sourcePos_og = atk_targ.sourcePos;
        
        // declare info that doesn't need to be in a loop
        float target_dist = (targetPos_og - sourcePos_og).magnitude;
        float og_speed = typeData.projectile_speed;
        Vector2 target_dir = (targetPos_og - sourcePos_og).normalized;
        float target_ang = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < typeData.projectile_count; i++)
        {
            TargetData atk_targ_copy = atk_targ;
            Vector2 sourcePos = atk_targ_copy.sourcePos;
            
            GameObject projectile = GameObject.Instantiate(instance, sourcePos, Quaternion.identity);
            ProjectileBehavior projectile_data = projectile.GetComponent<ProjectileBehavior>();

            // add the inherent inaccuracy value of projectile
            if (typeData.even_spread)
            {
                float angle_inc = typeData.projectile_spread / typeData.projectile_count;
                float offset_ang = (-typeData.projectile_spread / 2f) + (angle_inc * i);
                float final_ang = target_ang + offset_ang;

                Vector2 dir = new Vector2(Mathf.Cos(final_ang * Mathf.Deg2Rad),Mathf.Sin(final_ang * Mathf.Deg2Rad));
                
                atk_targ_copy.targetPos = sourcePos + dir * target_dist;
            }
            else
            {
                Vector2 og_targ_pos = atk_targ_copy.targetPos;
                atk_targ_copy.targetPos += Random.insideUnitCircle * target_dist * typeData.projectile_spread/360;
                
                typeData.projectile_speed *= Mathf.Clamp(1 - (atk_targ_copy.targetPos - og_targ_pos).magnitude / target_dist, 0.5f, 1);
            }
            // new projectile! 
            projectile_data.StartProjectile(this, atk_targ_copy);
            typeData.projectile_speed = og_speed;
        }
    }

    public override float GetAtkSpread() {return typeData.projectile_spread;}
}
#endregion

#region Linecast
// linecasts occur once a projectile has exceeded a certain speed
[System.Serializable]
public class Linecast : Projectile
{
    #region Initializers
    // empty constructor
    public Linecast(GameObject instance = null) : base(instance) {}
    public Linecast(GameObject instance, AttackStats atk_stats, ProjectileTypeData typeData)
     : base(instance, atk_stats, typeData){}
    #endregion

    public override void Attack(TargetData atk_targ)
    {
        Vector2 targetPos_og = atk_targ.targetPos;
        Vector2 sourcePos_og = atk_targ.sourcePos;
        
        // declare info that doesn't need to be in a loop
        float target_dist = (targetPos_og - sourcePos_og).magnitude;
        Vector2 target_dir = (targetPos_og - sourcePos_og).normalized;
        float target_ang = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < typeData.projectile_count; i++)
        {
            TargetData atk_targ_copy = atk_targ;
            Vector2 sourcePos = atk_targ_copy.sourcePos;
            
            GameObject linecast = GameObject.Instantiate(instance, sourcePos, Quaternion.identity);
            LinecastBehavior linecast_data = linecast.GetComponent<LinecastBehavior>();

            // add the inherent inaccuracy value of projectile
            if (typeData.even_spread)
            {
                float angle_inc = typeData.projectile_spread / typeData.projectile_count;
                float offset_ang = (-typeData.projectile_spread / 2f) + (angle_inc * i);
                float final_ang = target_ang + offset_ang;

                Vector2 dir = new Vector2(Mathf.Cos(final_ang * Mathf.Deg2Rad),Mathf.Sin(final_ang * Mathf.Deg2Rad));
                
                atk_targ_copy.targetPos = sourcePos + dir * target_dist;
            }
            else
            {
                atk_targ_copy.targetPos += Random.insideUnitCircle * target_dist * typeData.projectile_spread/360;
            }
            // new linecast! 
            linecast_data.StartLinecast(this, atk_targ_copy);
        }
    }

    public override float GetAtkSpread() {return typeData.projectile_spread;}
}
#endregion
#region Melee Attack
[System.Serializable]
public class MeleeAttack : AttackObject
{
    [ShowIf("instance")] public MeleeTypeData typeData;

    #region Initializers
    // empty constructor
    public MeleeAttack(GameObject instance = null) : base(instance) {}
    
    public MeleeAttack(GameObject instance, AttackStats atk_stats, MeleeTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }
    #endregion

    public override void Attack(TargetData atk_targ)
    {
        Vector2 targetPos_og = atk_targ.targetPos;
        Vector2 sourcePos_og = atk_targ.sourcePos;   
    
        // declare info that doesn't need to be in a loop
        float target_dist = (targetPos_og - sourcePos_og).magnitude;
        Vector2 target_dir = (targetPos_og - sourcePos_og).normalized;
        float target_ang = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        for (int i = 0; i < typeData.melee_count; i++)
        {
            TargetData atk_targ_copy = atk_targ;
            Vector2 sourcePos = atk_targ_copy.sourcePos;
            
            GameObject melee_ins = GameObject.Instantiate(instance, sourcePos, Quaternion.identity);
            MeleeBehavior melee_data = melee_ins.GetComponent<MeleeBehavior>();

            // add the inherent inaccuracy value of projectile
            if (typeData.even_spread)
            {
                float angle_inc = typeData.melee_spread / (typeData.melee_count - 1);
                float offset_ang = (-typeData.melee_spread / 2f) + (angle_inc * i);
                float final_ang = target_ang + offset_ang;

                Vector2 dir = new Vector2(Mathf.Cos(final_ang * Mathf.Deg2Rad),Mathf.Sin(final_ang * Mathf.Deg2Rad));
                
                atk_targ_copy.targetPos = sourcePos + dir * target_dist;
            }
            else
            {
                atk_targ_copy.targetPos += Random.insideUnitCircle * target_dist * typeData.melee_spread/360;
            }
            // new projectile! 
            melee_data.StartMelee(this, atk_targ_copy);
        }
    }

    public override float GetAtkSpread() {return typeData.melee_spread;}
}
#endregion