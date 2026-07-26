using UnityEngine;
using System;

[System.Serializable]
public class ProjectileTypeData
{
    public float projectile_speed;
    public float homing_radius;
    public float HomingSpdScale;
    public int projectile_count = 1;
    public float projectile_spread;
    public bool even_spread;
}

[System.Serializable]
public class MeleeTypeData
{
    public float melee_duration;
    public int melee_count = 1;
    public float melee_spread;
    public bool even_spread;
    public float melee_size = 1;
}