using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AttackSystem;

public class ProjectileBehavior : MonoBehaviour
{
    [field: Header("Target")]
    TargetData _targetData;

    [field: Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;
    //public ImpactEffect impact_effect;

    float stick_duration; // stick to a target for a set duration

    [field: Header("Projectile Data")]
    AttackStats atk_stats;
    public float speed;
    public float HomingSpdScale;

    [field: Header("Physics")]
    public Rigidbody2D proj_rb;
    float travel_time;
    float curr_travel_time = 0;

    [field: Header("Ownership")]
    Character _owner = null;
    string object_tag = "Untagged";

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag != "NoHit" && collider.gameObject != _owner.gameObject)
        {
            bool destroyObject = false;
            if (collider.gameObject.layer == 6)
            {
                if (atk_stats.bounce > 0)
                {                    
                    RaycastHit2D hit = Physics2D.Raycast(proj_rb.position - proj_rb.linearVelocity * Time.fixedDeltaTime, proj_rb.linearVelocity.normalized, speed, (1 << 6));
                    if (hit.collider != null)
                    {
                        proj_rb.position = hit.point;
                        proj_rb.linearVelocity = Vector2.Reflect(proj_rb.linearVelocity, hit.normal);

                        RotateToVelocity();
                    }
                    
                    // Vector2 directionToOther = (collider.ClosestPoint(transform.position) - proj_rb.position - proj_rb.linearVelocity*Time.fixedDeltaTime).normalized;
                    // proj_rb.linearVelocity = Vector2.Reflect(proj_rb.linearVelocity.normalized, -directionToOther) * speed;
                    // Debug.Log(proj_rb.linearVelocity.normalized);
                    // Debug.DrawLine(proj_rb.position, proj_rb.position + directionToOther * 3, Color.aquamarine, 3);
                    // Debug.DrawLine(collider.ClosestPoint(transform.position), collider.ClosestPoint(transform.position) + Vector2.up, Color.red, 3);
                    
                    travel_time += 1;
                    atk_stats.bounce--;
                    
                }
                else
                {
                    destroyObject = true;
                }
            }
            
            if (collider.gameObject.TryGetComponent<Character>(out Character character))
            {
                
                if (character.FactionTag == object_tag)
                {
                    return;
                }

                atk_stats.pierce--;
                if (atk_stats.pierce <= 0)
                {
                    destroyObject = true;
                }
            }

            atk_stats.ApplyData(_targetData.sourcePos, collider.gameObject);
            ProjectileEffects(collider.ClosestPoint(transform.position), destroyObject);
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // constantly readjust velocity for homing projectiles
        if (_targetData.objectTarget) {
            Vector2 targetDir = ((Vector2)_targetData.objectTarget.position - proj_rb.position).normalized;
            proj_rb.linearVelocity = Vector2.Lerp(proj_rb.linearVelocity.normalized, targetDir, speed * HomingSpdScale * Time.fixedDeltaTime) * speed;

            RotateToVelocity();
        }

        // check if destination has been reached?
        curr_travel_time += Time.fixedDeltaTime;
        if (curr_travel_time >= travel_time)
        {
            ProjectileEffects(transform.position, true);
        }

        // set vfx
        vfx_body.position = Vector2.MoveTowards(vfx_body.position, proj_rb.position + _targetData.vfxTargetOffset, travel_time * Time.fixedDeltaTime * 2);

    }

    // causes the projectile to rotate in the direction it is flying
    private void RotateToVelocity()
    {
        Vector2 target_dir = proj_rb.linearVelocity.normalized;
        float angle = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void StartProjectile(Projectile proj_data, TargetData atk_targ) // straight shot variant
    {
        atk_stats = proj_data.atk_stats;
        speed = proj_data.typeData.projectile_speed;
        HomingSpdScale = proj_data.typeData.HomingSpdScale;

        _targetData = atk_targ; 
        if (_targetData.owner)
        {
            _owner = _targetData.owner;
            object_tag = _targetData.owner.gameObject.tag;
        }

        // adjust vfx rotation
        Vector2 target_dir = _targetData.GetDir().normalized;
        float angle = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);

        // adjust vfx height from vfx body
        vfx_body.position = atk_targ.vfxSourcePos;

        // move this thing
        proj_rb.linearVelocity = target_dir * speed;

        // set travel time to know when to terminate the projectile
        float distance = _targetData.GetDir().magnitude;
        travel_time = distance / speed * 3;
    } 

    private void ProjectileEffects(Vector2 effect_position, bool terminate = false)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, vfxTargetOffset, targetPos - sourcePos, main_body.transform.localScale.x);
        if (terminate)
        {
            EndProjectile();
        }
    }

    private void EndProjectile()
    {
        Destroy(this.gameObject);
    }
}
