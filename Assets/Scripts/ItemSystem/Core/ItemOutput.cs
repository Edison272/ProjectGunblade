using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AttackSystem;
using UnityEditor;
using UnityEngine.Rendering;
using Unity.IO.LowLevel.Unsafe;


public enum ItemOutputType {Gun, Melee, Shield, Caster}

/// <summary>
/// Connects functionality to modules
/// Subscribes to different input events, and determines how they interact with stacks and attacking
/// </summary>

[Serializable]
public abstract class ItemOutput
{
    public ItemOutputType itemOutputType;
    protected Item _baseItem;
    [SerializeReference] public int[] attackObjectRefs = new int[] {}; // contains a custom collection of index references for attack types
    [SerializeReference] public int[] stackCounterRefs = new int[] {}; // contains a custom collection of index references for stack counters
    // THE FIRST ITEM OF STACK COUNTER REFS IS THE MOST IMPORTANT. THAT IS THE ONE WHICH ACTIVATES THE ITEM EFFECT WHEN TRIGGERED
    [SerializeField] protected AnimationRequest _animationRequest; // when making items, each animation
    public InputEventSelector InputEvent; // mostly just for show and vfx stuff
    public InputEventSelector OutputEvent; // reads stack inputs to get an output when called

    #region Initializer
    public ItemOutput() {}
    // return a deep copy of this item effect
    public ItemOutput(ItemOutput copiedOutput, Item baseItem)
    {
        _baseItem = baseItem;
        // attack types and refs will never change
        attackObjectRefs = copiedOutput.attackObjectRefs;
        stackCounterRefs = copiedOutput.stackCounterRefs;

        InputEvent = copiedOutput.InputEvent;
        OutputEvent = copiedOutput.OutputEvent;
        
        // _animation requests don't change either. just keep a reference
        this._animationRequest = copiedOutput._animationRequest;
        
    }
    // clean copy function
    public abstract ItemOutput GetCopy(Item baseItem);

    public virtual ItemOutput SetInputRelay(InputEventRelay inputRelay)
    {
        inputRelay.ConnectEvent(OutputEvent.InputEvent, ActivateEffect);
        return this;
    }
    #endregion
    public float CheckStackCounters()
    {
        float stack_index = 1;
        foreach(int stack_ref in stackCounterRefs)
        {
            stack_index *= _baseItem.stackCounters[stack_ref].GetIndexData();
            if (stack_index < 0)
            {
                return -1;
            }
        }
        return stack_index;
    }
    public float GetReadinessValue()
    {
        float greatestTime = 0;
        foreach(int stack_ref in stackCounterRefs)
        {
            float curr = _baseItem.stackCounters[stack_ref].GetReadinessTime();
            if (curr > greatestTime)
                greatestTime = curr;
        }
        return greatestTime; // more readiness time, less readiness score
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
        if (attack_ref_index > -1)
            _baseItem.UseItem(attackObjectRefs[attack_ref_index], _animationRequest);

        InternalActivateEffect(stack_counter, attack_ref_index);
    }

    // used to activate the internal effect
    protected abstract void InternalActivateEffect(float stackCounterVal, int attackRefIndex);
    public abstract bool NeedsReset();

    #region Recast Support
    // returns the expecected stack count type enum based on the type of the class
    public ItemOutputType GetExpectedItemOutputType()
    {
        return this switch
        {
            GunOutput => ItemOutputType.Gun,
            MeleeOutput => ItemOutputType.Melee,
            ShieldOutput => ItemOutputType.Shield,
            CasterOutput => ItemOutputType.Caster,
            _ => ItemOutputType.Gun
        };
    }
    public ItemOutput SmartRecast()
    {
        ItemOutput new_type = this;
        switch(itemOutputType)
        {
            case ItemOutputType.Gun:
                new_type = new GunOutput();
                break;
            case ItemOutputType.Melee:
                new_type = new MeleeOutput();
                break;
            case ItemOutputType.Shield:
                new_type = new ShieldOutput();
                break;
            case ItemOutputType.Caster:
                new_type = new CasterOutput();
                break; 
        }
        return new_type;
    }
    #endregion

    #region Helpers
    // returns a stack counter given an index
    protected virtual StackCounter GetStackCounter(int index)
    {
        if (index >= 0 && index < _baseItem.stackCounters.Length)
            return _baseItem.stackCounters[index];
        else
            return null;
    }
    protected virtual AttackObject GetAttackObject(int index)
    {
        if (index >= 0 && index < _baseItem.baseData.AttackObjects.Length)
            return _baseItem.baseData.AttackObjects[index];
        else
            return null;
    }
    #endregion

    #region GUI Helper
    // this MUST be called by another function, otherwise there will be a problem
    public void OnValidate()
    {

    }

    #endregion
}