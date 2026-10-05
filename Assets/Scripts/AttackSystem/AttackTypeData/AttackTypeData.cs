using UnityEngine;
using System;


[System.Serializable]
public class ProjectileTypeData
{
    public float projectile_speed = 10;
    public float homing_radius;
    public float HomingSpdScale = 1;
    public int projectile_count = 1;
    public float projectile_spread;
    public bool even_spread;
}

[Serializable]
public class LinecastTypeData
{
    public float render_duration = 0.1f; // was projectile_speed
    public int linecast_count  = 1;
    public float linecast_spread;
    public bool  even_spread;
}

[System.Serializable]
public class MeleeTypeData
{
    public float melee_duration = 0.5f;
    public int melee_count = 1;
    public float melee_spread;
    public bool even_spread;
    public float melee_size = 1;
}