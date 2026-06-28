using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemEffectInit
{
    [Header("Attack Type")]
    [Header("Input Listeners")]
    public bool effectOnStart; // activate effect on main input start
    public bool effectOnEnd; // activate effect on main input end
    public bool isAltEffect; // causes effect to occur on alt input instead of main input

    [Header("Effect Modifiers")]
    public bool Charging; // add effects to attack based on level of charge

}