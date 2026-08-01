// using System;

// [Serializable]
// public class CharStatModifier
// {
//     public int MaxHealth_boost = 0;
//     public int health_regen = 0;
//     public float regen_rate = 1f;
//     public int shield_boost = 0;
//     public float SpeedScale = 1;
//     public bool SpeedScale_decay = false;
//     public void ApplyStats(Character target, float stat_duration, AbilityEffectComponent effect_controller = null)
//     {
//         if (MaxHealth_boost > 0)
//         {
//             target.MaxHealthBoost(MaxHealth_boost, stat_duration, effect_controller);
//         }
//         if (health_regen != 0)
//         {
//             target.ChangeHealthTick(-health_regen, stat_duration, regen_rate, effect_controller);
//         }
//         if (shield_boost > 0)
//         {
//             target.ShieldBoost(shield_boost);
//         }
//         if (SpeedScale != 1)
//         {
//             target.ChangeSpeed(SpeedScale, stat_duration, SpeedScale_decay, effect_controller);
//         }
//     }
// }