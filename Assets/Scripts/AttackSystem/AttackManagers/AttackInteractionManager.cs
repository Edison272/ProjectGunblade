using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// Effectively an event bus to broadcast interaction events from attack
public static class AttackEventManager
{
    public static event Action<AttackObject> OnHit;
    
    public static void OnHitEvent(GameObject instance, AttackObject atkObj)
    {
        OnHit?.Invoke(atkObj);
    }
}
