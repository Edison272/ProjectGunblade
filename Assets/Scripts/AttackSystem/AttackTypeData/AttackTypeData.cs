using UnityEngine;
using System;

[Serializable]
public class AttackTypeData
{
    [Header("Basic Data")]
    public int Count = 1; // how many shots?
    public float Angle = 0; // the angular spread
    public bool Even = false; // true = even fan, false = scattered
    public float range = 20;
    public float size = 1;
}
[Serializable]
public class ProjectileTypeData : AttackTypeData
{
    [Header("Projectile Data")]
    public float projectile_speed = 10;
    public int pierce;
    public int bounce;
    [Range(0f, 1f)] public float speed_drift = 0;
    public float homing_radius;
    public float HomingSpdScale = 1;
}

[Serializable]
public class LinecastTypeData : AttackTypeData
{
    [Header("Linecast Data")]
    public int pierce;
    public int bounce;
    [Range(0f, 1f)] public float range_drift = 0;
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
    public float ActivationDelay = 0; // when the area will actually become effective
    public float AreaDuration = 0.5f; // 0 for instant
    public int ApplicationAmt = 1; // how many times the AOE is applied over the total duration
    public AttackObject AreaAttack = null; // Additional 

}