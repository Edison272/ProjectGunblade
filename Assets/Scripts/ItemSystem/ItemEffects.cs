using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AttackSystem;

/// <summary>
/// Connects functionality to modules
/// Subscribes to different input events, and determines how they interact with stacks and attacking
/// </summary>

[Serializable]
public class ItemEffect
{
    [SerializeReference] public int[] attackObjectRefs = new int[] {}; // contains a custom collection of index references for attack types
    [SerializeReference] public int[] stackCounterRefs = new int[] {}; // contains a custom collection of index references for stack counters

    // private references to the relevant collections
    private AttackObject[] attackObjects;
    private StackCounter[] stackCounters;

    // determine what input events need to be listened to based on stackCounterRefs
    public void SetupEventListeners(Character user)
    {
        // get an array of which input events are going to be needed
        bool[] input_events_used = new bool[(int)InputEvent.Size];
        foreach(StackCounter counter in stackCounters)
        {
            counter.GetEvents(input_events_used);
        }

        // use findings to connect them to array
    }

    public float CheckStackCounters()
    {
        float stack_index = -1;
        foreach(int stack_ref in stackCounterRefs)
        {
            stack_index = stackCounters[stack_ref].GetStackIndex();
        }
        return stack_index;
    }

    #region GUI Helper
    // this MUST be called by another function, otherwise there will be a problem
    public void OnValidate()
    {

    }

    #endregion
}