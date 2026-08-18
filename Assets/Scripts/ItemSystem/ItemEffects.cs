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

    [SerializeField] private AnimationRequest _animationRequest; // when making items, each animation

    #region Initializer
    // return a deep copy of this item effect
    public ItemEffect(ItemEffect copiedItem, Item baseItem)
    {
        this.baseItem = baseItem;
        // attack types and refs will never change
        attackObjectRefs = copiedItem.attackObjectRefs;
        stackCounterRefs = copiedItem.stackCounterRefs;
        baseItem.stackCounters?[stackCounterRefs[0]].AddActivator(ActivateEffect).SetAnimator(baseItem.animator);
        // _animation requests don't change either. just keep a reference
        this._animationRequest = copiedItem._animationRequest;
        
    }
    // clean copy function
    public ItemEffect GetCopy(Item baseItem)
    {
        return new ItemEffect(this, baseItem);
    }
    #endregion
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
        // Debug.Log(attack_ref_index);
        if (attack_ref_index > -1)
        {
            baseItem.UseItem(attackObjectRefs[attack_ref_index], _animationRequest);
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