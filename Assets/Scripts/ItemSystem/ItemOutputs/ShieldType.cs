using UnityEngine;
using System;
using System.Collections;

using Random = UnityEngine.Random;

/// <summary>
/// Constant counter will constantly call the activation function
/// Only when the constant counter is in a "started" state will it then allow for another thing to be called.
/// </summary>
[Serializable]
public class ShieldOutput : ItemOutput
{
    public int ShieldDurability;
    private InputEventSelector OutputEvent {get; set;} // reads stack inputs to get an output when called

    public ShieldOutput()
    {
        itemOutputType = ItemOutputType.Shield;
    }
    public ShieldOutput(ItemOutput copiedOutput, Item baseItem) : base(copiedOutput, baseItem)
    {
        itemOutputType = ItemOutputType.Shield;
    }
    public override ItemOutput GetCopy(Item baseItem)
    {
        return new ShieldOutput(this, baseItem);
    }

    public override ItemOutput SetInputRelay(InputEventRelay inputRelay)
    {
        base.SetInputRelay(inputRelay);
        inputRelay.ConnectEvent(OutputEvent.InputEvent, ActivateEffect);
        return this;
    }

    public override bool NeedsReset()
    {
        throw new NotImplementedException();
    }

    protected override void InternalActivateEffect(float stackCounterVal, int attackRefIndex)
    {

    }
    private void BlockEvent()
    {
        
    }
}
