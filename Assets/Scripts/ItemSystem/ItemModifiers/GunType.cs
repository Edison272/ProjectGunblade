using UnityEngine;
using System;
using System.Collections;


/*
ItemModifiers are classes contained by the Item itself.
Different modifiers will mechanically alter the item beyond its normal capabilities

- Guns: Ammo, Recoil
- Melee: Combo/Combo Reset, Stance
- Shields: Block, Fortify
- Casters: Aura Effect
*/
public interface ItemModifiers
{

}

public class GunModifier : ItemModifiers
{
    
}

public class MeleeModifier : ItemModifiers
{
    
}

public class ShieldModifer : ItemModifiers
{
    
}

public class CasteRModifier : ItemModifiers
{
    
}