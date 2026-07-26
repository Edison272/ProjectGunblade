using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// carried by bullets, melee, and anything that does damage
/// used to apply damage, effects, and stautses to others
namespace AttackSystem{

    [Serializable]
    public struct AttackStats
    {
        [Header("Stat Data")]
        // Attack on impact
        public int damage;
        public int pierce;
        [SerializeField] [Range(0f, 100f)] float knockback_amt;
        
        // status effect application
        [SerializeField] float effect_time;
        [SerializeField] [Range(0.01f, 10.0f)] float slow_amt;
        [SerializeField] int total_dot_damage;
        [SerializeField] int total_dot_ticks;


        public void ApplyData(Vector3 sourcePos, GameObject target)
        {
            target.GetComponent<IHealth>()?.ChangeHealth(damage);
            IMovement targ_move = target.GetComponent<IMovement>();
            if (knockback_amt > 0)
            {
                targ_move?.ForceMove((target.transform.position - sourcePos).normalized, knockback_amt * 10);
            }
            
            if (effect_time > 0)
            {
                if (slow_amt != 1)
                {
                    //targ_move?.ChangeSpeed(slow_amt, effect_time, false);
                }
                if (total_dot_damage != 0)
                {
                    float damage_per_tick = (float)total_dot_damage/total_dot_ticks;
                    float tick_rate = effect_time/total_dot_ticks;
                    // normalize tick rate if damage per tick is less than 1
                    if (Mathf.Abs(damage_per_tick) < 1)
                    {
                        float rescale = 1/damage_per_tick;
                        Debug.Log(rescale + ", dpt " + damage_per_tick);
                        damage_per_tick = 1;
                        tick_rate *= rescale;
                    }
                    //target.GetComponent<IHealth>()?.ChangeHealthTick((int)damage_per_tick, effect_time, tick_rate);
                }
            }
        }
    }
}