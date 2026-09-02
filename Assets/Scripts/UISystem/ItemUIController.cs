using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIController : MonoBehaviour
{
    [Header("UI Objects")]
    public Image item_sprite;
    public TMP_Text item_counter;
    public RectTransform reset_bar;
    [Header("UI - Recoil Circle")]
    [SerializeField] int circle_step = 100;


    [Header("UI StatUI")]
    public float StatBarHeight = 10;
    public float StatBarSpacing => UIBarGroup.transform.GetComponent<VerticalLayoutGroup>().spacing;
    public RectTransform UIBarGroup;
    public GameObject StatBarPrefab;
    public List<GameObject> StatBars = new List<GameObject>();


    public LineRenderer MainReticle;
    public LineRenderer OffsetReticle;

    [Header("Character")]
    public Character active_character = null;
    private InventorySlot _inventorySlot = null;
    private Item _selectedItem;


    // [Header("Gun UI")]

    Action UpdateUIType;

    public void SetActiveCharacter(Character active_character)
    {
        this.active_character = active_character;
    }

    void Update()
    {
        if (active_character)
        {
            DrawRecoilCircle(MainReticle, active_character.TargetAimPos, 0.1f, 20);
            DrawRecoilCircle(OffsetReticle, active_character.AimPosition, active_character.OffsetVec.magnitude * (active_character.AimPosition - active_character.Position).magnitude + 0.2f, 100);
            
            // // change how ui looks when weapons are switched
            if (_inventorySlot != active_character.Inventory.CurrentSlot)
            {
                _inventorySlot = active_character.Inventory.CurrentSlot;
                SetUI();
            }
            // routine ui update
            else
            {
                // UpdateUIType();
            }
        }
    }

    private void SetUI()
    {
        _selectedItem = active_character.Inventory.GetCurrentSlotItem();
        if (_selectedItem.baseData.ui_image)
        {
            item_sprite.sprite = _selectedItem.baseData.ui_image;
        }

        // stat bars
        foreach (GameObject bar in StatBars)
        {
            bar.SetActive(false);
        }
        for(int i = 0; i < _selectedItem.stackCounters.Length; i++)
        {
            if (StatBars.Count < i+1)
            {
                StatBars.Add(Instantiate(StatBarPrefab, UIBarGroup));
            }
            StatBars[i].SetActive(true);
            StatBars[i].GetComponent<StackBarUI>().StackCounter = _selectedItem.stackCounters[i];
        }
        UIBarGroup.sizeDelta = new Vector2(UIBarGroup.sizeDelta.x, StatBars.Count * (StatBarHeight + StatBarSpacing));

        // switch (active_character.main_item.func_module)
        // {
        //     case Gun:
        //         UpdateUIType = UpdateGunUI;
        //         break;
        //     case Melee:
        //         break;
        //     case Shield:
        //         break;
        //     case Conduit:
        //         break;
        // }
    }

    #region Gun Update UI

    public void UpdateGunUI()
    {
        // Gun gun_module =  null;//(Gun)ui_item.func_module;
        // if (ui_item.reset_timer > 0)
        // {
        //     reset_bar.localScale = new Vector3(1-ui_item.GetResetCompletion(), 1, 0);
        //     item_counter.text = string.Format("Reloading");
        // }
        // else
        // {
        //     item_counter.text = string.Format("{0} / {1}", gun_module.ammo, gun_module.max_ammo);
        //     reset_bar.localScale = new Vector3(ui_item.GetFunctionCompletion(), 1, 1);
        // }

        // Vector2 look_pos = active_character.GetPosition() + active_character.aim_dir + new Vector2(0, ui_item.y_offset);
        // float item_dist_mag = (ui_item.source_pos - ui_item.target_pos).magnitude;
        // float recoil_radius = gun_module.curr_recoil * item_dist_mag;
        // float innacuracy_radius = ui_item.GetInnacuracy()/180 * item_dist_mag;
        // DrawRecoilCircle(look_pos, recoil_radius + innacuracy_radius);
        // // if (gun_module.CanFunction() && gun_module.curr_recoil > 0)
        // // {
        // //     DrawRecoilCircle(look_pos, gun_module.curr_recoil);
        // // } else
        // // {
        // //     DrawRecoilCircle(look_pos, 0);
        // // }
        
    }

    public void DrawRecoilCircle(LineRenderer drawer, Vector3 circle_pos, float radius, int circleStep)
    {
        drawer.positionCount = circleStep;
 
        for(int s = 0; s < circleStep; s++)
        {
            float circ_progress = (float)s/(circleStep-1);
            float circ_rad = circ_progress * 2 * Mathf.PI;
            
            float xScaled = Mathf.Cos(circ_rad);
            float yScaled = Mathf.Sin(circ_rad);
 
            float x = radius * xScaled;
            float y = radius * yScaled;
            float z = 0;
            Vector3 currentPosition = circle_pos + new Vector3(x,y,z);
 
            drawer.SetPosition(s, currentPosition);
        }
    }

    #endregion
}