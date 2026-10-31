using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileBehavior : AttackBehaviorBase
{
    ProjectileTypeData _baseTypeData;
    [Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;

    [Header("Projectile Data")]
    float speed;
    int pierce;
    int bounce;
    // homing data
    static readonly Collider2D[] _buffer = new Collider2D[16];
    float _areaCheckTick => 2f / speed; //
    float _areaCheckTimer;

    [Header("Physics")]
    public Rigidbody2D ProjRB;
    public Collider2D ProjCollider;
    ContactFilter2D _filter;
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
            if (pierce <= 0) destroyObject = true;
            pierce--;
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
        // update values
        _prevPosition = ProjRB.position;
        if (_areaCheckTimer >= 0)
            _areaCheckTimer -= Time.fixedDeltaTime;
            if (_areaCheckTimer <= 0) 
            {
                CheckAirburst();
                CheckHoming();
                _areaCheckTimer += _areaCheckTick;
            }

        HomingMove();

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
        _baseTypeData = proj_data.SpecTypeData;

        Vector2 dir = atk_targ.GetDir().normalized;
        speed = _baseTypeData.projectile_speed * (1 + Random.Range(_baseTypeData.speed_drift, -_baseTypeData.speed_drift));
        pierce = _baseTypeData.pierce;
        bounce = _baseTypeData.bounce;
        _filter = targetData.GetContactFilter2D();

        targetData = targetData.WithTargetPos(atk_targ.sourcePos + dir * speed);
        transform.position = targetData.sourcePos;

        // vfx rotation & height
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);
        vfx_body.position = targetData.vfxSourcePos;

        // physics component
        ProjRB.includeLayers = targetData.targetMaskTerrain;
        ProjRB.excludeLayers = ~ProjRB.includeLayers;
        ProjRB.linearVelocity = dir * speed;
        ProjRB.position = targetData.sourcePos;
        RotateToVelocity();

        // lifetime & timers
        _travelTime = _baseTypeData.range / (speed+0.01f);
        _currTravelTime = 0;
        bool areaCheckNeeded = _baseTypeData.AirburstRange > 0 || _baseTypeData.HomingRadius > 0;
        _areaCheckTimer = areaCheckNeeded ? 0 : -1;
    }

    public override TargetData GetTarget()
    {
        return targetData.CopyToNewPositions(transform.position, (Vector2)transform.position + ProjRB.linearVelocity);
    }
    private void ProjectileEffects(Vector2 effect_position, bool terminate = false)
    {
        ImpactEffect.StartImpact(atkObject.vfx_impact, effect_position, vfx_body.position - main_body.transform.position, ProjRB.linearVelocity);
        if (terminate) EndAttack();
    }

    public override void SetAttackActive(bool is_active)
    {
        base.SetAttackActive(is_active);
        ProjRB.simulated = is_active;
        main_body.gameObject.SetActive(is_active);
        vfx_body.gameObject.SetActive(is_active);
    }

    #region Projectile Functionality

    public void CheckAirburst()
    {
        if (_baseTypeData.AirburstRange > 0)
        {
            int count = Physics2D.OverlapCircle(ProjRB.position, _baseTypeData.AirburstRange, _filter, _buffer);
            if (count > 0)
                ProjectileEffects(ProjRB.position, true);
        }
    }

    public void CheckHoming()
    {
        bool homingTimeActive = _currTravelTime >= _travelTime * _baseTypeData.HomingDelayScale && _currTravelTime <= _travelTime * _baseTypeData.HomingEndScale;
        if (_baseTypeData.HomingRadius > 0 && homingTimeActive)
        {
            int count = Physics2D.OverlapCircle(ProjRB.position, _baseTypeData.HomingRadius, _filter, _buffer);
            Transform new_target = null;
            float closest_sqr_mag = -Mathf.Infinity;
            for (int i = 0; i < count; i++)
            {
                if (_buffer[i] != null && ((Vector2)_buffer[i].transform.position - ProjRB.position).sqrMagnitude > closest_sqr_mag) {
                    new_target = _buffer[i].transform;
                }
            }
            if (new_target)
                targetData = targetData.WithObjectTarget(new_target);
        }
    }
    public void HomingMove()
    {
        // constantly readjust velocity for homing projectiles
        if (targetData.objectTarget)
        {
            Vector2 targetDir = ((Vector2)targetData.objectTarget.position - ProjRB.position).normalized;
            ProjRB.linearVelocity = Vector2.Lerp(ProjRB.linearVelocity.normalized, targetDir, speed * _baseTypeData.HomingSpdScale * Time.fixedDeltaTime) * speed;
            RotateToVelocity();
        }
    }

    #endregion
}