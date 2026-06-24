using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AttackSystem;

public class ProjectileBehavior : MonoBehaviour
{
    Vector2 source_pos;
    Transform target_char;
    Vector2 target_pos;
    Vector2 vfx_target_offset;

    [field: Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;
    //public ImpactEffect impact_effect;

    float stick_duration; // stick to a target for a set duration

    [field: Header("Projectile Data")]
    AttackStats atk_stats;
    public float speed;
    public int curr_pierce;

    [field: Header("Physics")]
    public Rigidbody2D proj_rb;
    float travel_time;
    float curr_travel_time = 0;

    [field: Header("Ownership")]
    string object_tag = "Untagged";
    int faction_tag = 1;
    Character owner;

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag != object_tag && collider.gameObject.tag != "NoHit")
        {
            atk_stats.ApplyData(this.main_body.transform.position, collider.gameObject);
            curr_pierce--;
            ProjectileEffects(collider.ClosestPoint(transform.position));
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (target_char) {
            target_pos = ((Vector2)target_char.position - proj_rb.position).normalized * speed;
            proj_rb.linearVelocity = Vector2.Lerp(proj_rb.linearVelocity, target_pos, speed * Time.fixedDeltaTime);
        }

        // check if destination has been reached?
        curr_travel_time += Time.fixedDeltaTime;
        if (curr_travel_time >= travel_time)
        {
            curr_pierce = 0;
            ProjectileEffects(transform.position);
        }

        // set vfx
        vfx_body.position = Vector2.MoveTowards(vfx_body.position, proj_rb.position + vfx_target_offset, travel_time * Time.fixedDeltaTime);

    }

    public void StartProjectile(Projectile proj_data, AttackTarget atk_targ) // straight shot variant
    {
        atk_stats = proj_data.atk_stats;
        speed = proj_data.typeData.projectile_speed;
        source_pos = atk_targ.source_pos;
        target_pos = atk_targ.target_pos;
        vfx_target_offset = atk_targ.vfx_target_offset;
        owner = atk_targ.sender;
        if (owner)
        {
            object_tag = owner.gameObject.tag;
            faction_tag = owner.faction_tag;
        }

        // adjust vfx rotation
        Vector2 target_dir = (target_pos - source_pos).normalized;
        float angle = Mathf.Atan2(target_dir.y, target_dir.x) * Mathf.Rad2Deg;
        vfx_body.rotation = Quaternion.Euler(0, 0, angle);

        // adjust vfx height from vfx body
        vfx_body.position = atk_targ.output_pos;

        // Vector2 source_offset = output_pos - target_dir * (output_pos.x - source_pos.x)/target_dir.x;
        // float offset_y = source_offset.y - source_pos.y;
        // vfx_body.localPosition = new Vector3(0, offset_y, 0);

        // adjust vfx rotation
        // Vector3 vfx_og_pos = vfx_body.transform.position;  // save original position for later
        // Quaternion targ_rot = Quaternion.LookRotation(Vector3.forward, target_pos - source_pos) * ROTATION_OFFSET;
        // main_body.transform.rotation = targ_rot;
        // vfx_body.transform.position = vfx_og_pos;  // return vfx_body to original position after offset from rotation
        // vfx_body.localPosition = new Vector3(0, vfx_body.localPosition.y, 0);

        // move this thing
        proj_rb.linearVelocity = (target_pos - source_pos).normalized * speed;

        // set travel time to know when to terminate the projectile
        float distance = Vector2.Distance(source_pos, target_pos);
        travel_time = distance / speed;

        curr_pierce = atk_stats.pierce+1;
    }

    public void StartProjectile(Transform target_char) // homing vairant
    {
        this.target_char = target_char;
    }

    private void ProjectileEffects(Vector2 effect_position)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, vfx_target_offset, target_pos - source_pos, main_body.transform.localScale.x);
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
