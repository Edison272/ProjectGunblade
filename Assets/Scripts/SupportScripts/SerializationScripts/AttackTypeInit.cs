using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AttackTypeInit // used to initialize ONE attack and its data
{
    public GameObject prefab = null;
    // public AttackEnum attack_enum = AttackEnum.Projectile;

    // [ShowIfAny("attack_enum", AttackEnum.Projectile, AttackEnum.Linecast)] public ProjectileTypeData projectile_data = new ProjectileTypeData();
    // [ShowIf("attack_enum", AttackEnum.MeleeAttack)] public MeleeTypeData melee_data = new MeleeTypeData();
    // public AttackData attack_data;

    // public AttackType CreateAttackType()
    // {
    //     AttackType atk_type = null;
    //     switch (attack_enum)
    //     {
    //         case AttackEnum.Projectile:
    //             atk_type = new Projectile(prefab,
    //                 attack_data,
    //                 projectile_data
    //             );
    //             break;
    //         case AttackEnum.Linecast:
    //             atk_type = new Linecast(prefab,
    //                 attack_data,
    //                 projectile_data
    //             );
    //             break;
    //         case AttackEnum.MeleeAttack:
    //             atk_type = new MeleeAttack(prefab,
    //                 attack_data,
    //                 melee_data
    //             );
    //             break;
    //     }
    //     return atk_type;
    // }
}