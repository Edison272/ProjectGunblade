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
    public int curr_pierce;

    [field: Header("Physics")]
    public Rigidbody2D proj_rb;
    float travel_time;
    float curr_travel_time = 0;

    [field: Header("Ownership")]
    string object_tag = "Untagged";

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag != "NoHit")
        {
            if (collider.gameObject.TryGetComponent<Character>(out Character character))
            {
                if (character.FactionTag == object_tag)
                {
                    return;
                }
            }
            atk_stats.ApplyData(_targetData.sourcePos, collider.gameObject);
            curr_pierce--;
            ProjectileEffects(collider.ClosestPoint(transform.position));
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        // constantly readjust velocity for homing projectiles
        if (_targetData.objectTarget) {
            Vector2 targetDir = ((Vector2)_targetData.objectTarget.position - proj_rb.position).normalized;
            proj_rb.linearVelocity = Vector2.Lerp(proj_rb.linearVelocity.normalized, targetDir, speed * HomingSpdScale * Time.fixedDeltaTime) * speed;

            Vector2 target_dir = proj_rb.linearVelocity.normalized;
            float angle = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
            vfx_body.rotation = Quaternion.Euler(0, 0, angle);
        }

        // check if destination has been reached?
        curr_travel_time += Time.fixedDeltaTime;
        if (curr_travel_time >= travel_time)
        {
            curr_pierce = 0;
            ProjectileEffects(transform.position);
        }

        // set vfx
        vfx_body.position = Vector2.MoveTowards(vfx_body.position, proj_rb.position + _targetData.vfxTargetOffset, travel_time * Time.fixedDeltaTime);

    }

    public void StartProjectile(Projectile proj_data, TargetData atk_targ) // straight shot variant
    {
        atk_stats = proj_data.atk_stats;
        speed = proj_data.typeData.projectile_speed;
        HomingSpdScale = proj_data.typeData.HomingSpdScale;

        _targetData = atk_targ; 
        if (_targetData.owner)
        {
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

        curr_pierce = atk_stats.pierce+1;
    } 

    private void ProjectileEffects(Vector2 effect_position)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, vfxTargetOffset, targetPos - sourcePos, main_body.transform.localScale.x);
        if (curr_pierce == 0)
        {
            EndProjectile();
        }
    }

    private void EndProjectile()
    {
        Destroy(this.gameObject);
    }
}
