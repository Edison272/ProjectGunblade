// using System;

// [Serializable]
// public class CharStatModifier
// {
//     public int max_health_boost = 0;
//     public int health_regen = 0;
//     public float regen_rate = 1f;
//     public int shield_boost = 0;
//     public float speed_scale = 1;
//     public bool speed_scale_decay = false;
//     public void ApplyStats(Character target, float stat_duration, AbilityEffectComponent effect_controller = null)
//     {
//         if (max_health_boost > 0)
//         {
//             target.MaxHealthBoost(max_health_boost, stat_duration, effect_controller);
//         }
//         if (health_regen != 0)
//         {
//             target.ChangeHealthTick(-health_regen, stat_duration, regen_rate, effect_controller);
//         }
//         if (shield_boost > 0)
//         {
//             target.ShieldBoost(shield_boost);
//         }
//         if (speed_scale != 1)
//         {
//             target.ChangeSpeed(speed_scale, stat_duration, speed_scale_decay, effect_controller);
//         }
//     }
// }