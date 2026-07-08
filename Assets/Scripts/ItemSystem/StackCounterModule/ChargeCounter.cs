using System;
using System.Collections;
using UnityEngine;

public class ChargeCounter : StackCounter
{
    [Header("Input Events")]
    [SerializeField] InputEvent useAmmoEvent = InputEvent.Character_MainStart;
    [SerializeField] InputEvent reloadAmmoEvent = InputEvent.Usable_Reset;
    #region Functionality

    #endregion

    #region Stack Status

    public override float GetStatus()
    {
        return 0.1f;
    }
    #endregion
}