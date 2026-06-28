using System;
using System.Collections;
using UnityEngine;

public class ChargeCounter : StackCounter
{
    [Header("Input Events")]
    [SerializeField] InputEvent useAmmoEvent = InputEvent.MainStart;
    [SerializeField] InputEvent reloadAmmoEvent = InputEvent.Reset;
    #region Functionality

    #endregion

    #region Stack Status
    public override bool IsReady()
    {
        return true;
    }

    public override float GetStatus()
    {
        return 0.1f;
    }
    #endregion
}