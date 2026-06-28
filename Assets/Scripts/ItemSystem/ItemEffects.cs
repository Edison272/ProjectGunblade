using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AttackSystem;

[Serializable]
public class ItemEffect
{
    [SerializeReference] public int[] attackObjectRefs = new int[] {}; // contains a custom collection of index references for attack types
    [SerializeReference] public int[] stackCounterRefs = new int[] {}; // contains a custom collection of index references for stack counters

    public void Attack(AttackTarget atk_targ)
    {

    }

    #region GUI Helper
    // this MUST be called by another function, otherwise there will be a problem
    public void OnValidate()
    {

    }

    #endregion
}