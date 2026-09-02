using UnityEngine;
using System;
using System.Collections;

using Random = UnityEngine.Random;


/*
ItemOutputs are classes contained by the Item itself.
Different Outputs will mechanically alter the item beyond its normal capabilities

- Guns: Ammo, Recoil
- Melee: Combo/Combo Reset, Stance
- Shields: Block, Fortify
- Casters: Aura Effect
*/
[Serializable]
public class GunOutput : ItemOutput
{
    [Header("Ammo")]
    public int AmmoCounterRef;
    public int AmmoUse;

    [Header("Recoil")]
    public float recoil_increment = 0.1f; // how much the aimed position is offset with each shot
    public float recoil_multiplier = 0.5f; // how much the aimed position is offset with each shot
    public float recoil_max = 1; // percent of distance
    private float _currRecoil = 0;
    public GunOutput()
    {
        itemOutputType = ItemOutputType.Gun;
    }
    public GunOutput(ItemOutput copiedOutput, Item baseItem) : base(copiedOutput, baseItem)
    {
        itemOutputType = ItemOutputType.Gun;
        
        GunOutput copiedGunOutput = (GunOutput)copiedOutput;
        
        AmmoCounterRef = copiedGunOutput.AmmoCounterRef;
        AmmoUse = copiedGunOutput.AmmoUse;
         
        recoil_increment = copiedGunOutput.recoil_increment; // how much the aimed position is offset with each shot
        recoil_multiplier = copiedGunOutput.recoil_multiplier; // how much the aimed position is offset with each shot
        recoil_max = copiedGunOutput.recoil_max; // percent of distance
    }
    public override ItemOutput GetCopy(Item baseItem)
    {
        return new GunOutput(this, baseItem);
    }

    public override bool NeedsReset()
    {
        StackCounter ammoCounter = GetStackCounter(AmmoCounterRef);
        if (ammoCounter != null)
        {
            return ammoCounter.GetIndexData() < 0;
        }
        return false;
    }

    protected override void InternalActivateEffect(float stackCounterVal, int attackRefIndex)
    {
        if (attackRefIndex > -1) 
        {
            _currRecoil = Mathf.Min(recoil_max, (_currRecoil  + recoil_increment) * recoil_multiplier);
            _baseItem.user.StaggerAim(Random.insideUnitCircle, _currRecoil);
            Debug.Log(NeedsReset());
            _baseItem.NeedsReset = NeedsReset();
        }
        
        
        
    }
}

public class MeleeOutput : ItemOutput
{
    public float DeflectCooldown;
    public MeleeOutput()
    {
        itemOutputType = ItemOutputType.Melee;
    }
    public MeleeOutput(ItemOutput copiedOutput, Item baseItem) : base(copiedOutput, baseItem)
    {
        itemOutputType = ItemOutputType.Melee;
    }
    public override ItemOutput GetCopy(Item baseItem)
    {
        return new MeleeOutput(this, baseItem);
    }

    public override bool NeedsReset()
    {
        throw new NotImplementedException();
    }

    protected override void InternalActivateEffect(float stackCounterVal, int attackRefIndex)
    {

    }
}

public class ShieldOutput : ItemOutput
{
    public int ShieldDurability;
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

    public override bool NeedsReset()
    {
        throw new NotImplementedException();
    }

    protected override void InternalActivateEffect(float stackCounterVal, int attackRefIndex)
    {

    }
}

public class CasterOutput : ItemOutput
{
    public float EffectRadius;
    public CasterOutput()
    {
        itemOutputType = ItemOutputType.Caster;
    }
    public CasterOutput(ItemOutput copiedOutput, Item baseItem) : base(copiedOutput, baseItem)
    {
        itemOutputType = ItemOutputType.Caster;
    }
    public override ItemOutput GetCopy(Item baseItem)
    {
        return new CasterOutput(this, baseItem);
    }

    public override bool NeedsReset()
    {
        throw new NotImplementedException();
    }

    protected override void InternalActivateEffect(float stackCounterVal, int attackRefIndex)
    {

    }
}