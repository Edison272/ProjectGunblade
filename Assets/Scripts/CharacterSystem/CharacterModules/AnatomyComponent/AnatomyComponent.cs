using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// VFX helper class
/// Anatomy components are responsible for locating the different parts of the body on any given character
/// This also controls what they realistically can/can't do
/// - How does a character aim?
/// - How does a character hold items in their inventory?
/// - What parts are animated at a time?
/// 
/// unlike other helpers, anatomy components are to be setup in the editor
/// 
/// 
/// NOTES
/// - Initialize all body parts in the editor. DO NOT INSTANTIATE THIS
/// - This is a HELPER CLASS TO CHARACTER.CS, but many of the methods here should be private
/// 
/// </summary>
[System.Serializable]
public class AnatomyComponent
{
    [field: Header("Body Parts")]
    public GameObject main_body;//basically the hitbox
    public GameObject vfx_body; //the vfx body
    public Transform front;
    public Transform back;
    public Transform body;
    public Transform body_sprite;
    public Transform body_outline;
    public Transform main_hand; //always set to main hand object 
    public Transform alt_hand; //always set to off hand object
    public Transform head;
    public Transform front_particles;
    public Transform back_particles;
    public Transform true_front;
    public Transform true_back;

    [field: Header("VFX")]
    public float base_sprite_height;
    public float base_head_height;
    public Animator animator;
    protected Vector2 sprite_center; // center of mass of this srpite
    private float hitbox_radius;
    // character has 4 VFX states based on aim direciton, stored as two booleans, X and Y
    // T, F = Right Bottom | T, T = Right Top | F, T = Left Top | F, F = Left Bottom
    protected (bool, bool) direction_state = (true, true);
    protected (Vector2, Vector2) akimbo_hand_pos = (new Vector2 (-0.2f, 0.6f), new Vector2 (0.5f, 0.6f));  // (main pos (left), alt pos (right))
    Vector2 single_hand_pos = new Vector2 (0, 0.6f);  // (main pos, alt pos)
    
    [field: Header("Aiming")]
    public Vector2 aim_dir {get; private set;} = Vector2.zero; // vector from operator to where they are looking. MAKE SURE ITS UN-NORMALIZED
    public Vector2 offset_look = Vector2.zero;
    protected Action AimStyle; // single-item or akimbo aiming?
    public  float aim_angle = 0; // angle (deg) the character is looking in
    readonly Vector2 SingleWeaponRestPosition = new Vector2(1, -1);

    private Rigidbody2D entity_rb;

    #region Initalizers
    // called on awake by the character class
    public void Setup(Rigidbody2D entity_rb, Animator animator)
    {
        this.entity_rb = entity_rb;
        this.animator = animator;
        
        hitbox_radius = main_body.GetComponent<CircleCollider2D>().radius;
        akimbo_hand_pos = ((Vector2) main_hand.localPosition, (Vector2) alt_hand.localPosition);
        single_hand_pos = new Vector2(0, main_hand.localPosition.y);

        base_sprite_height = body_sprite.GetComponent<SpriteRenderer>().bounds.size.y;
        base_head_height = head.localPosition.y;

        // set basic sibling order of entity VFX (operator faces BOTTOM RIGHT by default)
        animator.SetBool("FaceFront", true);
        main_hand.SetSiblingIndex(4);
        front.SetSiblingIndex(3);
        vfx_body.transform.SetSiblingIndex(2);
        back.SetSiblingIndex(1);
        alt_hand.SetSiblingIndex(0);


    }

    public void IdlePosition()
    {
        SetAimStyle(true);
        // initialize default look position
        aim_dir = SingleWeaponRestPosition;
        Look(entity_rb.position + aim_dir);
    }


    #endregion

    #region Looking & Aiming
    public void Look(Vector2 look_pos) {
        // look_dir is the direction the operator is set to look at
        Vector2 look_dir = (look_pos - entity_rb.position).normalized;

        if ((look_dir.x >= 0) != (aim_dir.x >= 0))
        {
            Vector3 look_scale = new Vector3 ((int)Mathf.Sign(look_dir.x), 1, 1);
            front.localScale = look_scale;
            body.localScale = look_scale;
            back.localScale = look_scale;
            akimbo_hand_pos.Item1.x *= -1;
            akimbo_hand_pos.Item2.x *= -1;
        }


        // aim hands and body to correct direction
        aim_dir = look_pos - entity_rb.position;
        AimStyle();
    }
    public void SetAimStyle(bool is_akimbo) // set the position of main & alt hands for akimbo or non-akimbo weaponry whenever weapon switch
    {
        if (is_akimbo)
        {
            // set hand index
            main_hand.SetSiblingIndex(direction_state.Item1? 4 : 0);
            alt_hand.SetSiblingIndex(direction_state.Item1? 0 : 4);
            // adjust hand positions to either side of body
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;
            AimStyle = AkimboAim;
        } else {
            // set hand index
            main_hand.SetSiblingIndex(direction_state.Item2? 4 : 0);
            alt_hand.SetSiblingIndex(direction_state.Item2? 0 : 4);
            // adjust hand positions to center mass
            main_hand.localPosition = single_hand_pos;
            alt_hand.localPosition = single_hand_pos;
            AimStyle = SingleAim;
        }
    }
    void AkimboAim() // aim two weapons from two sides of body
    {
        // check if direction state has changed
        if (direction_state.Item1 != aim_dir.x > 0) // direction_state.Item1 = true -> facing right
        {
            direction_state.Item1 = aim_dir.x > 0; // update direction state
            
            // switch hand indexes
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(main_hand.GetSiblingIndex() == 4 ? 0 : 4);

            // switch hand positions
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;
        }

        if (direction_state.Item2 != aim_dir.y < 0) // direction_state.Item2 = true -> facing down (front)
        {
            direction_state.Item2 = aim_dir.y < 0; // update direction state
            
            // switch front & back index
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front.GetSiblingIndex() == 3 ? 1 : 3);

            // switch hand positions
            main_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item1 : akimbo_hand_pos.Item2;
            alt_hand.localPosition = direction_state.Item2 == direction_state.Item1? akimbo_hand_pos.Item2 : akimbo_hand_pos.Item1;

            animator.SetBool("FaceFront", direction_state.Item2); // face front
        }
    }
    void SingleAim() // aim one weapon from center of mass
    {   
        if(direction_state.Item2 != aim_dir.y <= 0) {
            direction_state.Item2 = aim_dir.y <= 0; // update direction state

            // switch hand  indexes
            main_hand.SetSiblingIndex(alt_hand.GetSiblingIndex());
            alt_hand.SetSiblingIndex(main_hand.GetSiblingIndex() == 4 ? 0 : 4);   
            // switch front/back indexes
            front.SetSiblingIndex(back.GetSiblingIndex());
            back.SetSiblingIndex(front.GetSiblingIndex() == 3 ? 1 : 3);

            animator.SetBool("FaceFront", direction_state.Item2); // face front
        }
    }

    #endregion

    #region VFX Body
    public virtual Transform GetBodyPart(CharacterBodyPart body_part_type) {
        Transform body_part = main_body.transform;
        switch(body_part_type)
        {
            case CharacterBodyPart.Hitbox:
                body_part = main_body.transform;
                break;
            case CharacterBodyPart.TrueFront:
                body_part = true_front;
                break;
            case CharacterBodyPart.TrueBack:
                body_part = true_back;
                break;
            case CharacterBodyPart.Front:
                body_part = front;
                break;
            case CharacterBodyPart.Back:
                body_part = back;
                break;
            case CharacterBodyPart.SpriteBody:
                body_part = body_sprite;
                break;
            case CharacterBodyPart.MainHand:
                body_part = main_hand;
                break;
            case CharacterBodyPart.AltHand:
                body_part = alt_hand;
                break;
            case CharacterBodyPart.Head:
                body_part = head;
                break;
            case CharacterBodyPart.FrontParticles:
                body_part = front_particles;
                break;
            case CharacterBodyPart.BackParticles:
                body_part = back_particles;
                break;
        }
        return body_part;
    }

    public void PlaceOnBody(CharacterBodyPart body_part_type, Transform place_object)
    {
        Transform body_part = GetBodyPart(body_part_type);
        place_object.transform.SetParent(body_part, true);
    }

    public void UpdateBodyVFX()
    {
        //body_outline.GetComponent<SpriteRenderer>().sprite = body_sprite.GetComponent<SpriteRenderer>().sprite;
        float curr_sprite_height = body_sprite.GetComponent<SpriteRenderer>().bounds.size.y;
        head.transform.localPosition = new Vector3(0, base_head_height * curr_sprite_height/base_sprite_height, 0);
    }

    public void SetOutlineAlpha(float alpha)
    {
        Color o_c = body_outline.GetComponent<SpriteRenderer>().color;
        body_outline.GetComponent<SpriteRenderer>().color = new Color(o_c.r, o_c.b, o_c.g, alpha);
    }

    public Transform GetHand(bool is_main)
    {
        return is_main ? main_hand : alt_hand;
    }
    #endregion
}