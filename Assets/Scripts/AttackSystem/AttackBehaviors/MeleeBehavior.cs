using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AttackSystem;

public class MeleeBehavior : MonoBehaviour
{
    Vector2 sourcePos;
    Transform target_char;
    Vector2 targetPos;
    Vector2 end_pos; // if the Melee stops at whatever it hits (or the last thing it hits if it can pierce)
    Vector2 vfxTargetOffset;

    [field: Header("VFX")]
    public GameObject main_body;
    public Transform vfx_body;
    public SpriteRenderer sprt_rendr;
    public Sprite[] melee_sprites;
    float render_duration;
    float curr_duration;
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90); // RotateTowards() is stupid so we need to offset it


    float stick_duration; // stick to a target for a set duration

    [field: Header("Line Data")]
    AttackStats atk_stats;


    [field: Header("Physics")]
    RaycastHit2D[] contacts;



    [field: Header("Ownership")]
    string object_tag = "Untagged";
    int faction_tag = 1;
    Character owner;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != object_tag && collision.gameObject.tag != "NoHit")
        {
            atk_stats.ApplyData(sourcePos, collision.gameObject);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curr_duration -= Time.fixedDeltaTime;
        if (curr_duration <= 0)
        {
            EndMelee();
        }
        else
        {
            int next_sprite = (int)(melee_sprites.Length * (curr_duration / render_duration));
            sprt_rendr.sprite = melee_sprites[next_sprite];
        }
    }

    public void StartMelee(MeleeAttack mele_data, TargetData atk_targ) // straight shot variant
    {
        // set data
        atk_stats = mele_data.atk_stats;
        render_duration = mele_data.typeData.melee_duration;
        curr_duration = render_duration;
        sourcePos = atk_targ.sourcePos;
        targetPos = atk_targ.targetPos;
        end_pos = targetPos;
        vfxTargetOffset = atk_targ.vfxTargetOffset;
        owner = atk_targ.owner;
        if (owner)
        {
            object_tag = owner.gameObject.tag;
            faction_tag = owner.faction_tag;
        }
        // adjust size & position based on new size
        main_body.transform.localScale = main_body.transform.localScale * mele_data.typeData.melee_size;
        main_body.transform.position = sourcePos + (targetPos - sourcePos).normalized * mele_data.typeData.melee_size * 0.1f;

        // adjust vfx height from vfx body
        vfx_body.position = atk_targ.vfxSourcePos;

        // adjust vfx rotation
        Vector3 vfx_og_pos = vfx_body.transform.position;  // save original position for later
        Quaternion targ_rot = Quaternion.LookRotation(Vector3.forward, targetPos - sourcePos) * ROTATION_OFFSET;
        main_body.transform.rotation = targ_rot;
        vfx_body.transform.position = vfx_og_pos;  // return vfx_body to original position after offset from rotation
        vfx_body.localPosition = new Vector3(0, vfx_body.localPosition.y, 0);


    }

    public void StartMelee(Transform target_char) // homing vairant
    {
        this.target_char = target_char;
    }

    private void MeleeEffects()
    {
        EndMelee();
    }

    private void EndMelee()
    {
        Destroy(this.gameObject);
    }
}