using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ItemPickup : MonoBehaviour, IInteractable
{
    public ItemSO new_item;
    public Item used_item;
    [Header("Interaction Prompt")]
    public GameObject InteractionUI;
    SpriteRenderer this_sprite;

    public void SetItem(ItemSO set_new)
    {
        new_item = set_new;
    }

    public void Awake()
    {
        this_sprite = this.GetComponent<SpriteRenderer>();
    }
    public void Start()
    {
        
        if (new_item)
        {
            this_sprite.sprite = new_item.ui_image;
            //InteractionUI.SetActive(false);
        }

    }
    public void Interact(Character character)
    {        
        Debug.Log(character);
        Item item_pickup = used_item ? used_item : new_item.GenerateItem(transform.position);
        used_item = character.PickupItem(item_pickup);
        if (used_item)
        {        
            this_sprite.enabled = true;
            this_sprite.sprite = used_item.baseData.ui_image;
        }
        else
        {
            this_sprite.enabled = false;
        }
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