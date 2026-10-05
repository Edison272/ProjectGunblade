using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/// Pool for all attack behaviors, keyed by prefab (not by behavior type:
/// many different prefabs share ProjectileBehavior).
public static class AttackBehaviorPool
{
    const int MaxPerPrefab = 64;

    static readonly Dictionary<AttackBehaviorBase, Stack<AttackBehaviorBase>> pools =
        new Dictionary<AttackBehaviorBase, Stack<AttackBehaviorBase>>();
    static Transform root;

    // Root is destroyed with its scene (or when leaving play mode), taking pooled objects with it.
    // A destroyed root reads as null, so stale entries are dropped lazily; no scene hooks needed.
    static void EnsureRoot()
    {
        if (root) return;
        pools.Clear();
        root = new GameObject("AttackPool").transform;
    }

    // preemptively add prefabs to the pool
    public static void Prewarm(AttackBehaviorBase prefab, int count)
    {
        EnsureRoot();
        Stack<AttackBehaviorBase> stack = GetStack(prefab);
        for (int i = 0; i < count && stack.Count < MaxPerPrefab; i++)
        {
            AttackBehaviorBase b = Create(prefab);
            if (!b) return;
            b.gameObject.SetActive(false);
            stack.Push(b);
        }
    }

    // Get or create a new attack.
    public static AttackBehaviorBase GetAttack(AttackBehaviorBase prefab, Vector2 position)
    {
        EnsureRoot();
        Stack<AttackBehaviorBase> stack = GetStack(prefab);

        AttackBehaviorBase new_instance = null;
        while (stack.Count > 0 && !new_instance)
        {
            new_instance = stack.Pop(); // skip entries destroyed externally
        } 
        if (!new_instance) new_instance = Create(prefab);

        return new_instance;
    }

    // Instead of destroying an attack instance, recycle it
    public static void RemoveAttack(AttackBehaviorBase remove_instance)
    {
        if (!remove_instance.gameObject.activeSelf) return; // double-release guard

        if (!remove_instance.PoolKey || !pools.TryGetValue(remove_instance.PoolKey, out var stack))
        {
            Object.Destroy(remove_instance.gameObject);
            return;
        }
        stack.Push(remove_instance);
    }

    static Stack<AttackBehaviorBase> GetStack(AttackBehaviorBase prefab)
    {
        if (!pools.TryGetValue(prefab, out Stack<AttackBehaviorBase> stack))
        {
            stack = new Stack<AttackBehaviorBase>();
            pools[prefab] = stack;
        }
        return stack;
    }

    // create a fresh AttackBehavior when one is not present
    static AttackBehaviorBase Create(AttackBehaviorBase prefab)
    {
        AttackBehaviorBase new_instance = Object.Instantiate(prefab.gameObject, root).GetComponent<AttackBehaviorBase>();
        new_instance.PoolKey = prefab;
        return new_instance;
    }
}