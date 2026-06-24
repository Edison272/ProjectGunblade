using UnityEngine;
using System;

[System.Serializable]
public class ProjectileTypeData
{
    public float projectile_speed;
    public float homing_strength;
    public int projectile_count;
    public float projectile_spread;
    public bool even_spread;
}

[System.Serializable]
public class MeleeTypeData
{
    public float melee_duration;
    public int melee_count;
    public float melee_spread;
    public bool even_spread;
    public float melee_size;
}