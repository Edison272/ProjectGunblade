using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

// used for impact effects, explosions,and AOE
public class AreaEffectBehavior : AttackBehaviorBase
{
    [Header("VFX")]
    public GameObject MainBody;
    public Transform VFXBody;

    AreaEffectTypeData _baseTypeData;


    float PulseT(float total, int intervals) => intervals == 1 ? 0f : total / (intervals - 1);
    [Header("AreaEffect Pulse")]
    int _remainingPulses = 0;
    public float _currTimer = -1; // keeps track of when an AreaEffect "pulses". -1 means it is inactive
    [Header("AreaEffect Contact")]

    [Header("Physics")]
    public Collider2D ProjCollider;
    public readonly HashSet<GameObject> targets = new HashSet<GameObject>();

    void OnTriggerEnter2D(Collider2D collider)
    {
        GameObject other = collider.gameObject;
        if (IsOwner(other) || IsFriendly(other)) return;

        targets.Add(other);
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        GameObject other = collider.gameObject;
        if (targets.Contains(other))
            targets.Remove(other);
    }

    void FixedUpdate()
    {
        if (_currTimer > 0)
        {
            _currTimer -= Time.fixedDeltaTime;
            if (_currTimer <= _remainingPulses * PulseT(_baseTypeData.AreaDuration, _baseTypeData.ApplicationAmt))
           {
                _remainingPulses -= 1;
                AreaEffects(transform.position);
                ApplyHit(null);
                foreach(GameObject target in targets)
                {
                    ApplyHit(target);
                }
            } 


            if (_currTimer <= 0)
            {

                AreaEffects(transform.position, true);

                if (_remainingPulses <= 0)
                    _remainingPulses = 0;
                    _currTimer = -1;

            }
        }
    }
    void Update()
    {

    }

    public void StartAreaEffect(AreaEffect areaData, TargetData atkTarg)
    {
        
        Initialize(areaData, atkTarg);
        _baseTypeData = areaData.SpecTypeData;

        // targetData = atkTarg.WithTargetPos(Vector2.ClampMagnitude(atkTarg.GetDir(), _baseTypeData.range));
        MainBody.transform.position = targetData.targetPos;
        MainBody.transform.localScale = Vector2.one * _baseTypeData.size;

        ProjCollider.includeLayers = targetData.targetMask;
        ProjCollider.excludeLayers = ~ProjCollider.includeLayers;

        // effect pulses
        _remainingPulses = _baseTypeData.ApplicationAmt;
        _currTimer = 0.00001f + _baseTypeData.AreaDuration + _baseTypeData.ActivationDelay;

        // vfx rotation & height
        // Vector2 dir = targetData.GetDir().normalized;
        // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // vfx_body.rotation = Quaternion.Euler(0, 0, angle);
        // vfx_body.position = atkTarg.vfxSourcePos;

        if (targets.Count > 0)
            targets.Clear();
    }
    private void AreaEffects(Vector2 effect_position, bool terminate = false)
    {
        //ImpactEffect.StartImpact(impact_effect, effect_position, ...);
        if (terminate) EndAttack();
    }
    public override void SetAttackActive(bool is_active)
    {
        base.SetAttackActive(is_active);
        ProjCollider.enabled = is_active;
        MainBody.gameObject.SetActive(is_active);
        VFXBody.gameObject.SetActive(is_active);
    }
}