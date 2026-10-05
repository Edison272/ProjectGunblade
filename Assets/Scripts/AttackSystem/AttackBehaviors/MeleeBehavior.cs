using UnityEngine;

public class MeleeBehavior : AttackBehaviorBase
{
    [Header("VFX")]
    public GameObject main_body;
    public Transform VfxBody;
    public SpriteRenderer sprt_rendr;
    public Sprite[] melee_sprites;
    float render_duration;
    float curr_duration;
    static readonly Quaternion ROTATION_OFFSET = Quaternion.Euler(0, 0, 90); // RotateTowards() is stupid so we need to offset it

    [Header("Physics")]
    Rigidbody2D melee_rb;

    void Awake()
    {
        melee_rb = GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject other = collision.gameObject;
        if (IsOwner(other) || IsFriendly(other)) return;
        ApplyHit(other);
    }

    void Update()
    {
        // play sprites forward over the duration; clamp so the first tick can't index past the end
        float progress = 1f - curr_duration / render_duration;
        int next_sprite = Mathf.Min((int)(melee_sprites.Length * progress), melee_sprites.Length - 1);
        sprt_rendr.sprite = melee_sprites[next_sprite];
    }

    void FixedUpdate()
    {
        curr_duration -= Time.fixedDeltaTime;
        if (curr_duration <= 0)
        {
            EndAttack();
            return;
        }
    }

    public void StartMelee(MeleeAttack mele_data, TargetData atk_targ)
    {
        Initialize(mele_data, atk_targ);

        render_duration = mele_data.typeData.melee_duration;
        curr_duration = render_duration;
        melee_rb.includeLayers = atk_targ.targetMask;
        melee_rb.excludeLayers = ~melee_rb.includeLayers;

        Vector2 sourcePos = targetData.sourcePos;
        Vector2 targetPos = targetData.targetPos;
        float size = mele_data.typeData.melee_size;

        // adjust size & position based on new size
        main_body.transform.localScale = main_body.transform.localScale * Mathf.Abs(size);
        VfxBody.localScale = new Vector3(VfxBody.localScale.x, VfxBody.localScale.y * Mathf.Sign(size), 1);
        main_body.transform.position = sourcePos + (targetPos - sourcePos).normalized * Mathf.Abs(size) * 0.25f;

        // adjust vfx height from vfx body
        VfxBody.position = atk_targ.vfxSourcePos;

        // adjust rotation, then restore VfxBody's offset from the rotation
        Vector3 vfx_og_pos = VfxBody.position;
        main_body.transform.rotation = Quaternion.LookRotation(Vector3.forward, targetPos - sourcePos) * ROTATION_OFFSET;
        VfxBody.position = vfx_og_pos;
        VfxBody.localPosition = new Vector3(0, VfxBody.localPosition.y, 0);
    }

    public override void SetAttackActive(bool is_active)
    {
        base.SetAttackActive(is_active);
    }
}