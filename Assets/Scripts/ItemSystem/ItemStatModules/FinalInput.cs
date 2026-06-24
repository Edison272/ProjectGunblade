// using UnityEngine;
// using System;
// using ItemInputModules;
// using System.Collections.Generic;

// namespace ItemStatModules
// {
//     [System.Serializable]
//     public class SerializeFinalInput // launch an attack after the user inputs
//     {
//         [Header("Basic Stats")]
//         public float minimum_charge;
//         [ShowIf("extra_charge_states")] public AttackTypeInit[] charge_states;

//         public void AddInputModules(ItemInputController input_controller)
//         {
//             // if (main_attack_type != null)
//             // {
//             //     input_controller.AddInputModule(new ConstantInputModule(input_controller, hold_attack_speed, tap_attack_speed));
//             // }
//         }
//     }
// }