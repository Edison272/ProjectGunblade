using UnityEngine;

public class LinecastBehavior : AttackBehaviorBase
{
    Vector2 end_pos; // where the linecast stops (last thing it pierces, or the target pos)

    [Header("VFX")]
    public LineRenderer main_line_render; // shows where the actual linecast goes
    public LineRenderer vfx_line_render;  // shows where the vfx linecast goes
    //public ImpactEffect impact_effect;
    float main_lr_alpha; // base alpha for fade
    float render_duration;
    float curr_duration;

    void GenerateLinecast()
    {
        main_lr_alpha = main_line_render.startColor.a;

        RaycastHit2D[] contacts = Physics2D.LinecastAll(targetData.sourcePos, targetData.targetPos);
        int curr_pierce = atk_stats.pierce + 1;
        foreach (RaycastHit2D contact in contacts)
        {
            GameObject other = contact.transform.gameObject;
            if (other.CompareTag("NoHit") || IsOwner(other) || IsFriendly(other)) continue;

            ApplyHit(other);
            LinecastEffects(contact.point);
            curr_pierce--;
            if (curr_pierce == 0)
            {
                end_pos = contact.point;
                break;
            }
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
        Vector2 render_pos = Vector2.Lerp(vfx_line_render.GetPosition(0), end_pos + targetData.vfxTargetPos, 1 - curr_duration / render_duration);
        SetLRPositions(1, end_pos, render_pos);
    }

    void FixedUpdate()
    {
        curr_duration -= Time.fixedDeltaTime;

        if (curr_duration <= 0)
        {
            if (end_pos == targetData.targetPos) LinecastEffects(targetData.targetPos);
            EndAttack();
        }
    }

    public void StartLinecast(Linecast line_data, TargetData atk_targ)
    {
        Initialize(line_data.atk_stats, atk_targ);

        render_duration = line_data.typeData.render_duration; // used as fade duration
        curr_duration = render_duration;
        end_pos = targetData.targetPos;

        // generate the physics linecast (may shorten end_pos)
        GenerateLinecast();

        // origin of line renders
        SetLRPositions(0, atk_targ.sourcePos, atk_targ.vfxSourcePos);
        SetLRPositions(1, atk_targ.sourcePos, atk_targ.vfxSourcePos);
    }

    private void LinecastEffects(Vector2 effect_position)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, ...);
    }

    public override void SetAttackActive(bool is_active)
    {
        base.SetAttackActive(is_active);
    }
}