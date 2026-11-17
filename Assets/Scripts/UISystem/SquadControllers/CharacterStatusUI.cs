using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusUI : MonoBehaviour
{
    [SerializeField] Character _activeCharacter;
    [Header("Deployment Status")]
    [SerializeField] GameObject _activeUI;
    [SerializeField] GameObject _inactiveUI;
    [SerializeField] TextMeshProUGUI _redeployText;
    [Header("Operator Profile")]
    [SerializeField] TextMeshProUGUI _charName;
    [SerializeField] RectTransform _charImage;
    [Header("Selection")]
    [SerializeField] GameObject _selector;
    [SerializeField] TextMeshProUGUI _selectorKeybind;
    [Header("Ability")]
    [SerializeField] RectTransform _cooldownMask;
    [SerializeField] GameObject _readyVFX;

    public void Awake()
    {

    }
    public void SetActiveCharacter(Character setActiveChar)
    {
        _activeCharacter = setActiveChar;
        _charName.text = setActiveChar.Name;
        
        ToggleSelection(false);
    }

    public void ToggleSelection(bool is_selected)
    {
        if (!_activeCharacter)
            return;
        _selector.SetActive(is_selected);
        _activeCharacter.SetHighlight(is_selected);
    }

    public void Update()
    {
        if (!_activeCharacter)
            return;

        if (_activeCharacter.is_alive != _activeUI.activeSelf)
        {
            _activeUI.SetActive(_activeCharacter.is_alive);
            _inactiveUI.SetActive(!_activeCharacter.is_alive);
        }

        // if (_activeCharacter.is_alive)
        // {
        //     operator_active_ui.SetActive(true);
        //     operator_inactive_ui.SetActive(false);

        //     float cd_progress = active_operator.ability_cooldown_progress;
        //     cooldown_mask.localScale = new Vector3(1, cd_progress, 1);
        //     if (cd_progress >= 1 != _readyVFX.activeSelf)
        //     {
        //         _readyVFX.SetActive(cd_progress >= 1);
        //     }
        // }
        // else if (active_operator.curr_redeployment_time > 0)
        // {
        //     operator_active_ui.SetActive(false);
        //     operator_inactive_ui.SetActive(true);
        //     redeployment_text.text = "Redeploy: " + active_operator.curr_redeployment_time.ToString("F1");
        //     active_operator.curr_redeployment_time -= Time.deltaTime;
        // }
    }
}