using UnityEngine;
using System;

public class SequenceCounter : StackCounter
{
    [Header("Input Events")]
    [SerializeField] InputEvent useAmmoEvent = InputEvent.MainStart;
    [SerializeField] InputEvent reloadAmmoEvent = InputEvent.Reset;
    #region Functionality

    #endregion

    #region Stack Status

    public override float GetStatus()
    {
        return 0.1f;
    }
    #endregion
}