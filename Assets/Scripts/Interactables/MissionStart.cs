using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The MissionStart interactable opens a ui menu when interacted with
/// </summary>
public class MissionStart : MonoBehaviour, IInteractable
{

    [Header("Interaction Prompt")]
    public GameObject InteractionUI;
    public void Awake()
    {
    }
    public void Start()
    {
        
    }
    public void Interact(Character character)
    {        

    }

    public void ToggleInteractPrompt(bool is_enabled)
    {
        InteractionUI.SetActive(is_enabled);
    }

    public string GetPromptText()
    {
        return "pick up item";
    }
}