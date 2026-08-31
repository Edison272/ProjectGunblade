// using System;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class ItemUIController : MonoBehaviour
// {
//     [Header("UI Objects")]
//     public Image item_sprite;
//     public TMP_Text item_counter;
//     public RectTransform reset_bar;
//     [Header("UI - Recoil Circle")]
//     [SerializeField] int circle_step = 100;

//     public LineRenderer recoil_circle;

//     [Header("Character")]
//     public Character active_character = null;
//     private (int, int) curr_items = (-1,-1);
//     private Item ui_item;

//     Action UpdateUIType;

//     public void SetActiveCharacter(Character active_character)
//     {
//         this.active_character = active_character;
//     }

//     void Update()
//     {
//         if (active_character)
//         {
//             // change how ui looks when weapons are switched
//             if (curr_items != active_character.Inventory. || ui_item != active_character.main_item)
//             {
//                 SetUI();
//                 curr_items = active_character.current_indexes;
//             }
//             // routine ui update
//             else
//             {
//                 UpdateUIType();
//             }
//         }
//     }

//     private void SetUI()
//     {
//         if (active_character.main_item.ui_image)
//         {
//             item_sprite.sprite = active_character.main_item.ui_image;
//             ui_item = active_character.main_item;
//         }
//         // switch (active_character.main_item.func_module)
//         // {
//         //     case Gun:
//         //         UpdateUIType = UpdateGunUI;
//         //         break;
//         //     case Melee:
//         //         break;
//         //     case Shield:
//         //         break;
//         //     case Conduit:
//         //         break;
//         // }
//     }

//     #region Gun Update UI

//     public void UpdateGunUI()
//     {
//         Gun gun_module =  null;//(Gun)ui_item.func_module;
//         if (ui_item.reset_timer > 0)
//         {
//             reset_bar.localScale = new Vector3(1-ui_item.GetResetCompletion(), 1, 0);
//             item_counter.text = string.Format("Reloading");
//         }
//         else
//         {
//             item_counter.text = string.Format("{0} / {1}", gun_module.ammo, gun_module.max_ammo);
//             reset_bar.localScale = new Vector3(ui_item.GetFunctionCompletion(), 1, 1);
//         }

//         Vector2 look_pos = active_character.GetPosition() + active_character.aim_dir + new Vector2(0, ui_item.y_offset);
//         float item_dist_mag = (ui_item.source_pos - ui_item.target_pos).magnitude;
//         float recoil_radius = gun_module.curr_recoil * item_dist_mag;
//         float innacuracy_radius = ui_item.GetInnacuracy()/180 * item_dist_mag;
//         DrawRecoilCircle(look_pos, recoil_radius + innacuracy_radius);
//         // if (gun_module.CanFunction() && gun_module.curr_recoil > 0)
//         // {
//         //     DrawRecoilCircle(look_pos, gun_module.curr_recoil);
//         // } else
//         // {
//         //     DrawRecoilCircle(look_pos, 0);
//         // }
        
//     }

//     public void DrawRecoilCircle(Vector3 circle_pos, float radius)
//     {
//         recoil_circle.positionCount = circle_step;
 
//         for(int s = 0; s < circle_step; s++)
//         {
//             float circ_progress = (float)s/(circle_step-1);
//             float circ_rad = circ_progress * 2 * Mathf.PI;
            
//             float xScaled = Mathf.Cos(circ_rad);
//             float yScaled = Mathf.Sin(circ_rad);
 
//             float x = radius * xScaled;
//             float y = radius * yScaled;
//             float z = 0;
//             Vector3 currentPosition = circle_pos + new Vector3(x,y,z);
 
//             recoil_circle.SetPosition(s, currentPosition);
//         }
//     }

//     #endregion
// }