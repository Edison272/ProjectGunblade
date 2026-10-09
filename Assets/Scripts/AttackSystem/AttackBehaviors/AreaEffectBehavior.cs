using UnityEngine;

// used for impact effects, explosions,and AOE
public class AreaEffectBehavior : AttackBehaviorBase
{
    [Header("VFX")]
    public GameObject MainBody;
    public Transform VFXBody;

    [Header("AreaEffect Status")]

    [Header("Physics")]
    public Rigidbody2D ProjRB;
    public Collider2D ProjCollider;
    float travel_time;
    float curr_travel_time = 0;

    void Update()
    {
        
    }
    public void StartAreaEffect(AreaEffect areaData, TargetData atkTarg)
    {
        Initialize(areaData, atkTarg);
        MainBody.transform.position = atkTarg.sourcePos;

        // vfx rotation & height
        // Vector2 dir = targetData.GetDir().normalized;
        // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // vfx_body.rotation = Quaternion.Euler(0, 0, angle);
        // vfx_body.position = atkTarg.vfxSourcePos;

        // lifetime
        // float distance = targetData.GetDir().magnitude;
        // travel_time = distance / speed * 3;
    }
    public override void SetAttackActive(bool is_active)
    {
        base.SetAttackActive(is_active);
        ProjRB.simulated = is_active;
        MainBody.gameObject.SetActive(is_active);
        VFXBody.gameObject.SetActive(is_active);
    }
}