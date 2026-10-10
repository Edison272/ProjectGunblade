using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

// used for impact effects, explosions,and AOE
public class AreaEffectBehavior : AttackBehaviorBase
{
    [Header("VFX")]
    public GameObject MainBody;
    public Transform VFXBody;

    AreaEffectTypeData _baseTypeData;

    // Growing/Shrinking the Area Effect

    // For "pulsing" effects which happen at fixed intervals
    float PulseT(float total, int intervals) => intervals == 1 ? 0f : total / (intervals - 1);
    int _remainingPulses = 0;
    float _currTimer = -1; // keeps track of when an AreaEffect "pulses". -1 means it is inactive

    // For "constant" effects which happen so long as the target is standing in the field
    

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

            // check pulsing
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
            }
        }
    }
    void Update()
    {
        if (_currTimer > 0 && _currTimer < _baseTypeData.GetTotalTime)
        {
            float lerp_amt =  1 - _currTimer / _baseTypeData.GetTotalTime;
            float curr_size = Mathf.Lerp(_baseTypeData.size, _baseTypeData.size * _baseTypeData.StartingScale, lerp_amt);
            MainBody.transform.localScale = Vector2.one * curr_size;
        }
    }

    public void StartAreaEffect(AreaEffect areaData, TargetData atkTarg)
    {
        
        Initialize(areaData, atkTarg);
        _baseTypeData = areaData.SpecTypeData;

        // targetData = atkTarg.WithTargetPos(Vector2.ClampMagnitude(atkTarg.GetDir(), _baseTypeData.range));
        MainBody.transform.position = targetData.targetPos;
        MainBody.transform.localScale = Vector2.one * _baseTypeData.size * _baseTypeData.StartingScale;

        ProjCollider.includeLayers = targetData.targetMask;
        ProjCollider.excludeLayers = ~ProjCollider.includeLayers;

        // effect pulses
        _remainingPulses = _baseTypeData.ApplicationAmt;
        _currTimer = _baseTypeData.GetTotalTime;

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