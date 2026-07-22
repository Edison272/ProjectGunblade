using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AttackSystem;

public class LinecastBehavior : MonoBehaviour
{
    Vector2 source_pos;
    Transform target_char;
    Vector2 target_pos;
    Vector2 end_pos; // if the linecast stops at whatever it hits (or the last thing it hits if it can pierce)
    Vector2 vfx_target_offset;

    [field: Header("VFX")]
    public LineRenderer main_line_render; // show where the actual linecast is going
    float main_lr_alpha; // used when setting the alpha color in FixedUpdate()
    public LineRenderer vfx_line_render; // show where the vfx linecast is going
    //public ImpactEffect impact_effect;
    float render_duration;
    float curr_duration;

    float stick_duration; // stick to a target for a set duration

    [field: Header("Line Data")]
    AttackStats atk_stats;


    [field: Header("Physics")]
    RaycastHit2D[] contacts;



    [field: Header("Ownership")]
    string object_tag = "Untagged";
    int faction_tag = 1;
    Character owner;

    void GenerateLinecast()
    {
        // get all targets hit in linecast
        contacts = Physics2D.LinecastAll(source_pos, target_pos);
        int curr_pierce = atk_stats.pierce+1;
        foreach(RaycastHit2D contact in contacts)
        {
            if (contact.transform.gameObject.tag != object_tag && contact.transform.gameObject.tag != "NoHit")
            {
                atk_stats.ApplyData(source_pos, contact.transform.gameObject);
                LinecastEffects(contact.point);
                curr_pierce--;
            }
            if (curr_pierce == 0)
            {
                end_pos = contact.point;
                break;
            }
        }

        // change linerender vfx accordingly\
        main_lr_alpha = main_line_render.startColor.a;
        

    }

    void SetLRPositions(int index, Vector2 main_position, Vector2 vfx_position)
    {
        main_line_render.SetPosition(index, main_position);
        vfx_line_render.SetPosition(index, vfx_position);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curr_duration -= Time.fixedDeltaTime;

        // fade out main linderender after being shot
        float alpha = main_lr_alpha * curr_duration/render_duration;
        main_line_render.startColor = new Color(main_line_render.startColor.r,main_line_render.startColor.g,main_line_render.startColor.b, alpha);
        main_line_render.endColor = new Color(main_line_render.endColor.r,main_line_render.endColor.g,main_line_render.endColor.b, alpha);

        // set line render length (temporary rendering method)
        Vector2 render_pos = Vector2.Lerp(vfx_line_render.GetPosition(0), end_pos + vfx_target_offset, 1-curr_duration/render_duration);
        SetLRPositions(1, end_pos, render_pos);
        if (curr_duration <= 0)
        {
            if (end_pos == target_pos) {LinecastEffects(target_pos);}
            EndLinecast();
        } 
    }

    public void StartLinecast(Linecast line_data, TargetData atk_targ) // straight shot variant
    {
        // set data
        atk_stats = line_data.atk_stats;
        render_duration = line_data.typeData.projectile_speed;
        curr_duration = render_duration;
        source_pos = atk_targ.source_pos;
        target_pos = atk_targ.target_pos;
        end_pos = target_pos;
        vfx_target_offset = atk_targ.vfx_target_offset;
        owner = atk_targ.sender;
        if (owner)
        {
            object_tag = owner.gameObject.tag;
            faction_tag = owner.faction_tag;
        }
        // generate the physics linecast (this also sets a new end_pos based on where the linecast hits)
        GenerateLinecast();

        // set origin position of "main" line render
        SetLRPositions(0, atk_targ.source_pos, atk_targ.output_pos);
        SetLRPositions(1, atk_targ.source_pos, atk_targ.output_pos);

    }

    private void LinecastEffects(Vector2 effect_position)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, vfx_target_offset, target_pos - source_pos, vfx_line_render.widthMultiplier * 4);
    }

    private void EndLinecast()
    {
        Destroy(this.gameObject);
    }
}