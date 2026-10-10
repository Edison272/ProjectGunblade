using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class LinecastBehavior : AttackBehaviorBase
{
    Vector2 end_pos; // where the linecast stops (last thing it pierces, or the target pos)

    [Header("VFX")]
    public LineRenderer main_line_render; // shows where the actual linecast goes
    public LineRenderer vfx_line_render;  // shows where the vfx linecast goes
    //public ImpactEffect impact_effect;
    float main_lr_alpha; // base alpha for fade
    [Header("Liencast Data")]
    int pierce;
    int bounce;
    float render_duration;
    float curr_duration;


    void GenerateLinecast()
    {
        main_lr_alpha = main_line_render.startColor.a;

        RaycastHit2D[] contacts = Physics2D.LinecastAll(targetData.sourcePos, targetData.targetPos, targetData.targetMaskTerrain);
        foreach (RaycastHit2D contact in contacts)
        {
            GameObject other = contact.transform.gameObject;
            bool isTerrain = (terrainMask & (1 << other.layer)) != 0;
            pierce--;
            if (isTerrain || pierce == 0)
            {
                end_pos = contact.point;
                break;
            }

            if (IsOwner(other) || IsFriendly(other)) continue;

            ApplyHit(other);
            LinecastEffects(contact.point);

        }
    }

    void SetLRPositions(int index, Vector2 main_position, Vector2 vfx_position)
    {
        main_line_render.SetPosition(index, main_position);
        vfx_line_render.SetPosition(index, vfx_position);
    }

    void Update()
    {
        // fade out main linerender after being shot
        float alpha = main_lr_alpha * curr_duration / render_duration;
        Color start = main_line_render.startColor;
        Color end = main_line_render.endColor;
        main_line_render.startColor = new Color(start.r, start.g, start.b, alpha);
        main_line_render.endColor = new Color(end.r, end.g, end.b, alpha);

            // set line render length (temporary rendering method)
        float p = 1 - curr_duration / render_duration;
        float head_t = Mathf.Clamp01(p / 0.5f);
        float tail_t = Mathf.Clamp01((p - 0.25f) / (1f - 0.25f));

        Vector2 a = targetData.vfxSourcePos;
        Vector2 b = end_pos + targetData.vfxTargetOffset;

        SetLRPositions(0, targetData.sourcePos, Vector2.Lerp(a, b, tail_t));
        SetLRPositions(1, end_pos, Vector2.Lerp(a, b, head_t));
    }

    void FixedUpdate()
    {
        curr_duration -= Time.fixedDeltaTime;

        if (curr_duration <= render_duration/2)
            LinecastEffects(end_pos);
        if (curr_duration <= 0)
        {
            EndAttack();
        }
    }

    public void StartLinecast(Linecast line_data, TargetData atk_targ)
    {
        // float range = line_data.SpecTypeData.range * (1 + Random.Range(line_data.SpecTypeData.range_drift, -line_data.SpecTypeData.range_drift));

        Initialize(line_data, atk_targ);
        end_pos = atk_targ.targetPos;

        pierce = line_data.SpecTypeData.pierce;
        bounce = line_data.SpecTypeData.bounce;

        // generate the physics linecast (may shorten end_pos)
        GenerateLinecast();

        // same speed as a full-range shot: duration scales with actual distance
        float actual_dist = Vector2.Distance(targetData.sourcePos, end_pos);
        render_duration = Mathf.Max(line_data.SpecTypeData.render_duration * actual_dist / line_data.TypeData.range, 0.0001f);
        curr_duration = render_duration;

        // origin of line renders
        SetLRPositions(0, atk_targ.sourcePos, atk_targ.vfxSourcePos);
        SetLRPositions(1, atk_targ.sourcePos, atk_targ.vfxSourcePos);
    }
    public override TargetData GetTarget()
    {
        return targetData.CopyToNewPositions(end_pos, end_pos + targetData.GetDir());
    }
    private void LinecastEffects(Vector2 effect_position)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, ...);
    }

    public override void SetAttackActive(bool is_active)
    {
        base.SetAttackActive(is_active);
        main_line_render.gameObject.SetActive(is_active);
        vfx_line_render.gameObject.SetActive(is_active);
    }
}