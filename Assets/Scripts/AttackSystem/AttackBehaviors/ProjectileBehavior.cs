using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProjectileBehavior : AttackBehaviorBase
{
    [Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;
    //public ImpactEffect impact_effect;

    [Header("Projectile Data")]
    float speed;
    int pierce;
    int bounce;
    float HomingSpdScale;

    [Header("Physics")]
    public Rigidbody2D ProjRB;
    public Collider2D ProjCollider;
    float _travelTime = 0;
    float _currTravelTime = -1;
    Vector2 _prevPosition;

    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject other = collider.gameObject;
        if (IsOwner(other) || IsFriendly(other)) return;

        bool destroyObject = false;
        bool isTerrain = (terrainMask & (1 << other.layer)) != 0;

        if (isTerrain)
        {
            if (bounce > 0)
            {
                ColliderDistance2D col_dist = collider.Distance(ProjCollider); // projCollider = this projectile's Collider2D
                if (col_dist.isOverlapped)
                {
                    // normal points from the wall toward the projectile
                    Vector2 normal = col_dist.normal;
                    Debug.DrawLine(ProjRB.position, ProjRB.position + normal * (-col_dist.distance + 0.01f), Color.red, 1);
                    ProjRB.position = _prevPosition + normal * (-col_dist.distance + 0.1f); // push out of wall
                    ProjRB.linearVelocity = Vector2.Reflect(ProjRB.linearVelocity, normal);
                    Debug.DrawLine(ProjRB.position, ProjRB.position + ProjRB.linearVelocity, Color.green, 1);
                    RotateToVelocity();
                }
                //_travelTime *= 1.25f; // slightly increase travel time after bounce
                bounce--;
            }
            else
            {
                destroyObject = true;
            }
        }
        else if (other.TryGetComponent<Character>(out _))
        {
            pierce--;
            if (pierce <= 0) destroyObject = true;
        }

        ApplyHit(other);
        ProjectileEffects(collider.ClosestPoint(transform.position), destroyObject);
    }

    void Update()
    {
        // set vfx
        vfx_body.position = Vector2.MoveTowards(vfx_body.position, ProjRB.position + targetData.vfxTargetOffset, _travelTime * Time.deltaTime * speed);
        
    }
    void FixedUpdate()
    {
        if (!attackEnabled) {return;}
        _prevPosition = ProjRB.position;
        // constantly readjust velocity for homing projectiles
        if (targetData.objectTarget)
        {
            Vector2 targetDir = ((Vector2)targetData.objectTarget.position - ProjRB.position).normalized;
            ProjRB.linearVelocity = Vector2.Lerp(ProjRB.linearVelocity.normalized, targetDir, speed * HomingSpdScale * Time.fixedDeltaTime) * speed;
            RotateToVelocity();
        }

        // terminate when lifetime is up
        _currTravelTime += Time.fixedDeltaTime;
        if (_currTravelTime >= _travelTime)
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

    public void StartProjectile(Projectile proj_data, TargetData atk_targ)
    {
        Initialize(proj_data, atk_targ);

        Vector2 dir = atk_targ.GetDir().normalized;
        speed = proj_data.SpecTypeData.projectile_speed * (1 + Random.Range(proj_data.SpecTypeData.speed_drift, -proj_data.SpecTypeData.speed_drift));
        pierce = proj_data.SpecTypeData.pierce;
        bounce = proj_data.SpecTypeData.bounce;
        HomingSpdScale = proj_data.SpecTypeData.HomingSpdScale;
        ProjRB.includeLayers = targetData.targetMask;
        ProjRB.excludeLayers = ~ProjRB.includeLayers;

        targetData = targetData.WithTargetPos(atk_targ.sourcePos + dir * speed);
        main_body.transform.position = targetData.sourcePos;

        // vfx rotation & height
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);
        vfx_body.position = targetData.vfxSourcePos;

        ProjRB.linearVelocity = dir * speed;
        RotateToVelocity();

        // lifetime
        _travelTime = proj_data.SpecTypeData.range / speed;
        _currTravelTime = 0;
    }

    public override TargetData GetTarget()
    {
        return targetData.CopyToNewPositions(transform.position, (Vector2)transform.position + ProjRB.linearVelocity);
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