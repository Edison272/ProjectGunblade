using UnityEngine;
using System;


public class AttackTypeData
{
    public int Count = 1; // how many shots?
    public float Angle = 0; // the angular spread
    public bool Even = false; // true = even fan, false = scattered
    [SerializeReference] public AttackObject OnHitAttack = new Projectile(); // Additional attack effect when an attack successfully lands
    [SerializeReference] public AttackObject OnDestroyAttack = new Projectile(); // A default attack effect when the attack expires 
}
[System.Serializable]
public class ProjectileTypeData : AttackTypeData
{
    public float projectile_speed = 10;
    public float homing_radius;
    public float HomingSpdScale = 1;
}

[Serializable]
public class LinecastTypeData : AttackTypeData
{
    public float render_duration = 0.1f; // was projectile_speed
}

[System.Serializable]
public class MeleeTypeData : AttackTypeData
{
    public float melee_duration = 0.5f;
    public float melee_size = 1;
}
[System.Serializable]
public class AreaEffectTypeData : AttackTypeData
{
    public float AreaDuration = 0.5f; // 0 for instant
    public int ApplicationAmt = 1; // how many times the AOE is applied over the total duration
    public AttackObject AreaAttack = null; // Additional 

}