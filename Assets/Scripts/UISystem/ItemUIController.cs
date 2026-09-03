using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    public List<(GameObject,StackBarUI)> ActiveStatBars = new List<(GameObject,StackBarUI)>(); // save the resource as item1 to re-add back to pool
    private Dictionary<GameObject, Queue<StackBarUI>> _unusedStatBars = new Dictionary<GameObject, Queue<StackBarUI>>(); // used for pooling resources

    [Header("Reticle")]
    public RectTransform Reticle;
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
            
            Reticle.position = active_character.TargetAimPos;
            
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

        // deactivate any active stat bars
        for (int i = ActiveStatBars.Count-1; i >= 0; i--)
        {
            (GameObject, StackBarUI) bar = ActiveStatBars[i];
            bar.Item2.gameObject.SetActive(false);
            _unusedStatBars[bar.Item1].Enqueue(bar.Item2);
            ActiveStatBars.RemoveAt(i);
        }
        for(int i = 0; i < _selectedItem.stackCounters.Length; i++)
        {
            if (!_selectedItem.stackCounters[i].HasUI)
                continue;
            
            GameObject uiType = _selectedItem.stackCounters[i].UISetting.LoadUI();
            StackBarUI newUI = null;
            if (!_unusedStatBars.ContainsKey(uiType))
            {
                _unusedStatBars[uiType] = new Queue<StackBarUI>();
            }
            if (_unusedStatBars[uiType].Count == 0)
            {
                newUI = Instantiate(uiType, UIBarGroup).GetComponent<StackBarUI>();
                _unusedStatBars[uiType].Enqueue(newUI);
                
            }
            else
            {
                newUI = _unusedStatBars[uiType].Dequeue();
            }
            ActiveStatBars.Add((uiType, newUI));

            newUI.gameObject.SetActive(true);
            newUI.StackCounter = _selectedItem.stackCounters[i];
            PlaceStatBar(_selectedItem.stackCounters[i],newUI);
        }
        UIBarGroup.sizeDelta = new Vector2(UIBarGroup.sizeDelta.x, ActiveStatBars.Count * (StatBarHeight + StatBarSpacing));

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

    // used by SetUI to place a StatBar in the proper area
    private void PlaceStatBar(StackCounter counter, StackBarUI ui)
    {
        switch (counter.UISetting.PlacementDirection)
        {
            case UIPlacement.Reticle_Left:
                ui.transform.SetParent(Reticle.GetChild(0), false);
                break;
            case UIPlacement.Reticle_Right:
                ui.transform.SetParent(Reticle.GetChild(1), false);
                break;
            case UIPlacement.Reticle_Up:
                ui.transform.SetParent(Reticle.GetChild(2), false);
                break;
            case UIPlacement.Reticle_Down:
                ui.transform.SetParent(Reticle.GetChild(3), false);
                break;
        }
        ui.transform.position = Vector3.zero;
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