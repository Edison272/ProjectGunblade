using UnityEngine;
using System;

[Serializable]
public class AttackTypeData
{
    [Header("Basic Data")]
    public int Count = 1; // how many shots?
    public float Angle = 0; // the angular spread
    public bool Even = false; // true = even fan, false = scattered
}
[Serializable]
public class ProjectileTypeData : AttackTypeData
{
    [Header("Projectile Data")]
    public float projectile_speed = 10;
    [Range(0f, 0.5f)] public float speed_drift = 0;
    public float projectile_range = 20;
    public float homing_radius;
    public float HomingSpdScale = 1;
}

[Serializable]
public class LinecastTypeData : AttackTypeData
{
    [Header("Linecast Data")]
    public float linecast_range = 20;
    [Range(0f, 0.5f)] public float range_drift = 0;
    public float render_duration = 0.1f; // was projectile_speed
}

[Serializable]
public class MeleeTypeData : AttackTypeData
{
    [Header("Melee Data")]
    public float melee_duration = 0.5f;
    public float melee_size = 1;
}
[Serializable]
public class AreaEffectTypeData : AttackTypeData
{
    [Header("Area Effect Data")]
    public float AreaDuration = 0.5f; // 0 for instant
    public int ApplicationAmt = 1; // how many times the AOE is applied over the total duration
    public AttackObject AreaAttack = null; // Additional 

}