using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AttackSystem;
using UnityEditor;
using UnityEngine.Rendering;

/// <summary>
/// Connects functionality to modules
/// Subscribes to different input events, and determines how they interact with stacks and attacking
/// </summary>

[Serializable]
public class ItemEffect
{
    public readonly Item baseItem;
    [SerializeReference] public int[] attackObjectRefs = new int[] {}; // contains a custom collection of index references for attack types
    [SerializeReference] public int[] stackCounterRefs = new int[] {}; // contains a custom collection of index references for stack counters
    // THE FIRST ITEM OF STACK COUNTER REFS IS THE MOST IMPORTANT. THAT IS THE ONE WHICH ACTIVATES THE ITEM EFFECT WHEN TRIGGERED

    [SerializeField] private string _animationKey = "Use"; // when making items, each animation

    public InputEvent ActivationEvent = InputEvent.Character_MainStart; // determines when the intem effect checks values

    #region Initializer
    // return a deep copy of this item effect
    public ItemEffect(ItemEffect copiedItem, Item baseItem)
    {
        this.baseItem = baseItem;
        // attack types and refs will never change
        attackObjectRefs = copiedItem.attackObjectRefs;
        stackCounterRefs = copiedItem.stackCounterRefs;
        baseItem.stackCounters[stackCounterRefs[0]].AddActivator(ActivateEffect);
        
    }
    // clean copy function
    public ItemEffect GetCopy(Item baseItem)
    {
        return new ItemEffect(this, baseItem);
    }
    #endregion

    // determine what input events need to be listened to based on stackCounterRefs
    public void SetupEventListeners(InputEventRelay inputRelay)
    {
        // get an array of which input events are going to be needed
        bool[] input_events_used = new bool[(int)InputEvent.Size];
        // foreach(StackCounter counter in stackCounters)
        // {
        //     counter.SetInputRelay(inputRelay);

            
        //     //counter.GetEvents(input_events_used);
        // }

        // use findings to connect them to array
    }

    public float CheckStackCounters()
    {
        float stack_index = 1;
        foreach(int stack_ref in stackCounterRefs)
        {
            stack_index *= baseItem.stackCounters[stack_ref].GetIndexData();
            if (stack_index < 0)
            {
                return -1;
            }
        }
        return stack_index;
    }

    // Activates this item's effect
    // checks the status of all stack counters to determine what attack to launch
    public void ActivateEffect()
    {
        float stack_counter = CheckStackCounters();
        if (stack_counter < 0)
        {
            return;
        }
        int attack_ref_index = (int)Mathf.Floor(stack_counter * (attackObjectRefs.Length-1));
        Debug.Log(attack_ref_index);
        if (attack_ref_index > -1)
        {
            baseItem.UseItem(attackObjectRefs[attack_ref_index], _animationKey);
        }
    }

    #region Helpers

    #endregion

    #region GUI Helper
    // this MUST be called by another function, otherwise there will be a problem
    public void OnValidate()
    {

    }

    #endregion
}