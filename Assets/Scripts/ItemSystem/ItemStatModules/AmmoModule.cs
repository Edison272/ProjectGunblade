using UnityEngine;
using System;

namespace ItemStatModules
{
    [System.Serializable]
    public class AmmoModule
    {
        [Header("Basic Stats")]
        public int max_ammo;
        public float reload_speed;

        [Header("Regen Ammo")]
        public bool regen_ammo;
        [ShowIf("regen_ammo")] public RegenAmmoModule regen_ammo_modifier;

        [Header("Rounds Reload")]
        public bool rounds_reload;
        [ShowIf("rounds_reload")] public int rounds_per_load = 1;
    }

    [System.Serializable]
    public class RegenAmmoModule
    {
        public int regen_ammo_delay = 1;
        public int regen_ammo_amt = 1;
        public float regen_ammo_spd;
    }
}