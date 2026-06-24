using UnityEngine;
using System;
//using ItemInputModules;
using System.Collections.Generic;

namespace ItemStatModules
{
    public enum ConstantInputVariantEnum {Normal, Increment, Sequence};
    [System.Serializable]
    public class SerializeConstantInput // launch an Attack during the user input
    {
        [Header("Basic Stats")]
        public bool is_full_auto;
        // Full-Auto - Hold down input to constantly fire
        [Range(0, 10)] public float hold_attack_speed;
        // Semi-Auto - Hold down input to fire at fixed rate, or spam-tap to fire faster
        [ShowIf("is_full_auto", false)] [Range(0, 10)] public float tap_attack_speed = -1f;
        public AttackTypeInit main_attack_init;
        public ConstantInputVariantEnum input_variant_enum = ConstantInputVariantEnum.Normal;
        [ShowIf("input_variant_enum", ConstantInputVariantEnum.Increment)] public IncrementingInput incrementing_attacks_module = new IncrementingInput();
        [ShowIf("input_variant_enum", ConstantInputVariantEnum.Sequence)] public SequenceInput sequence_attacks_module;

        // public void AddInputModules(ItemInputController input_controller)
        // {
        //     if (main_attack_init != null)
        //     {
        //         AttackType main_attack = main_attack_init.CreateAttackType();
        //         input_controller.AddInputModule(new ConstantInputModule(input_controller, main_attack, hold_attack_speed, tap_attack_speed));
        //     }
        //     switch(input_variant_enum)
        //     {
        //         case ConstantInputVariantEnum.Increment:
        //             break;
        //         case ConstantInputVariantEnum.Sequence:
        //             break;
        //     }
        // }
    }
    
    [System.Serializable]
    public class IncrementingInput
    {
        [Header("Incrementing Attacks")]
        public float max_increment_time;
        public AttackTypeInit[] attack_increments;
    }

    [System.Serializable]
    public class SequenceInput
    {
        [Header("Sequence Attacks")]
        public float max_increment_time;
        public AttackTypeInit[] attack_sequence;
        
    }
}