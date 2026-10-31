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
        [SerializeField] [Range(0f, 100f)] public float knockback_amt;
        
        // status effect application
        [SerializeField] public float effect_time;
        [SerializeField] [Range(0.01f, 10.0f)] public float slow_amt;
        [SerializeField] public int total_dot_damage;
        [SerializeField] public int total_dot_ticks;
    }
}