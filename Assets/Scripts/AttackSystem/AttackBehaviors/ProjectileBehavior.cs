using System;
using UnityEngine;

public class ProjectileBehavior : AttackBehaviorBase
{
    [Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;
    //public ImpactEffect impact_effect;

    [Header("Projectile Data")]
    public float speed;
    public float HomingSpdScale;

    [Header("Physics")]
    public Rigidbody2D ProjRB;
    public Collider2D ProjCollider;
    float travel_time;
    float curr_travel_time = 0;

    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject other = collider.gameObject;
        if (IsOwner(other) || IsFriendly(other)) return;

        bool destroyObject = false;
        bool isTerrain = (terrainMask & (1 << other.layer)) != 0;

        if (isTerrain)
        {
            if (atk_stats.bounce > 0)
            {
                ColliderDistance2D col_dist = collider.Distance(ProjCollider); // projCollider = this projectile's Collider2D
                if (col_dist.isOverlapped)
                {
                    // normal points from the wall toward the projectile
                    Vector2 normmal = col_dist.normal;
                    Debug.DrawLine(ProjRB.position, ProjRB.position + normmal * (-col_dist.distance + 0.01f), Color.red, 1);
                    ProjRB.position += normmal * (-col_dist.distance + 0.1f); // push out of wall
                    ProjRB.linearVelocity = Vector2.Reflect(ProjRB.linearVelocity, normmal);
                    Debug.DrawLine(ProjRB.position, ProjRB.position + ProjRB.linearVelocity, Color.green, 1);
                    RotateToVelocity();
                }
                travel_time *= 0.5f;
                curr_travel_time = 0;
                atk_stats.bounce--;
            }
            else
            {
                destroyObject = true;
            }
        }
        else if (other.TryGetComponent<Character>(out _))
        {
            atk_stats.pierce--;
            if (atk_stats.pierce <= 0) destroyObject = true;
        }

        ApplyHit(other);
        ProjectileEffects(collider.ClosestPoint(transform.position), destroyObject);
    }

    void Update()
    {
        // set vfx
        vfx_body.position = Vector2.MoveTowards(vfx_body.position, ProjRB.position + targetData.vfxTargetOffset, travel_time * Time.fixedDeltaTime * 2);
    }
    void FixedUpdate()
    {
        if (!attackEnabled) {return;}
        // constantly readjust velocity for homing projectiles
        if (targetData.objectTarget)
        {
            Vector2 targetDir = ((Vector2)targetData.objectTarget.position - ProjRB.position).normalized;
            ProjRB.linearVelocity = Vector2.Lerp(ProjRB.linearVelocity.normalized, targetDir, speed * HomingSpdScale * Time.fixedDeltaTime) * speed;
            RotateToVelocity();
        }

        // terminate when lifetime is up
        curr_travel_time += Time.fixedDeltaTime;
        if (curr_travel_time >= travel_time)
        {
            ProjectileEffects(transform.position, true);
        }
    }

    // rotate the vfx in the direction of flight
    private void RotateToVelocity()
    {
        Vector2 dir = ProjRB.linearVelocity.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void StartProjectile(Projectile proj_data, TargetData atk_targ, float speedScale = 1f)
    {
        Vector2 dir = atk_targ.GetDir().normalized;
        
        Initialize(proj_data, atk_targ);
        targetData = targetData.WithTargetPos(atk_targ.sourcePos * dir*speed);
        main_body.transform.position = targetData.sourcePos;

        speed = proj_data.typeData.projectile_speed * speedScale;
        HomingSpdScale = proj_data.typeData.HomingSpdScale;
        ProjRB.includeLayers = targetData.targetMask;
        ProjRB.excludeLayers = ~ProjRB.includeLayers;

        // vfx rotation & height
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);
        vfx_body.position = targetData.vfxSourcePos;

        ProjRB.linearVelocity = dir * speed;

        // lifetime
        travel_time = proj_data.typeData.projectile_range / speed;
    }

    private void ProjectileEffects(Vector2 effect_position, bool terminate = false)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, ...);
        if (terminate) EndAttack();
    }

    public override void SetAttackActive(bool is_active)
    {
        base.SetAttackActive(is_active);
        ProjRB.simulated = is_active;
        main_body.gameObject.SetActive(is_active);
        vfx_body.gameObject.SetActive(is_active);
    }
}