using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class ImpactEffect : MonoBehaviour
{
    public GameObject main_body;
    public GameObject vfx_body;
    ImpactEffect AbsolutePrefab;
    [SerializeField] float max_impact_time = 0.3f;
    [SerializeField] float curr_impact_time;
    void Awake()
    {
        Reset();
    }

    public void Reset()
    {
        main_body.SetActive(false);
    }

    public void Update()
    {
        if (curr_impact_time > 0)
        {
            curr_impact_time -= Time.deltaTime;
            if (curr_impact_time <= 0)
            {
                ImpactEffect.RemoveVFX(this);
                main_body.SetActive(false);
            }
        }
    }

    public void SetVFX(Vector3 setPos, Vector2 vfxPos, Vector2 direction, float scale_modifier)
    {
        curr_impact_time = max_impact_time;
        main_body.transform.localScale = AbsolutePrefab.main_body.transform.localScale * scale_modifier;
        main_body.transform.position = setPos;
        vfx_body.transform.localPosition = vfxPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        vfx_body.transform.rotation = Quaternion.Euler(0, 0, angle);

        
        main_body.SetActive(true);
    }

    #region Singleton Stuff
    private static readonly Dictionary<ImpactEffect, Stack<ImpactEffect>> impact_effect_pool = new Dictionary<ImpactEffect, Stack<ImpactEffect>>();
    static Transform root;

    // Root is destroyed with its scene (or when leaving play mode), taking pooled objects with it.
    // A destroyed root reads as null, so stale entries are dropped lazily; no scene hooks needed.
    static void EnsureRoot()
    {
        if (root) return;
        impact_effect_pool.Clear();
        root = new GameObject("VfxPool").transform;
    }
    public static void StartImpact(
        ImpactEffect instance_type, 
        Vector2 mainPos, 
        Vector2 vfxPos, 
        Vector2 dir, 
        float scale = 1)
    {
        EnsureRoot();
        Stack<ImpactEffect> stack = GetStack(instance_type);
        if (stack == null)
            return;
        // null safety check
        ImpactEffect new_instance = null;
        while (stack.Count > 0 && !new_instance)
            new_instance = stack.Pop(); // skip entries destroyed externally
        if (!new_instance) {
            new_instance = Create(instance_type);
        }
        new_instance.SetVFX(mainPos, vfxPos, dir, scale);
    }
    // Instead of destroying an attack instance, recycle it
    public static void RemoveVFX(ImpactEffect remove_instance)
    {
        if (!remove_instance.gameObject.activeSelf) return; // double-release guard

        if (!remove_instance.AbsolutePrefab || !impact_effect_pool.TryGetValue(remove_instance.AbsolutePrefab, out var stack))
        {
            Destroy(remove_instance.gameObject);
            return;
        }
        stack.Push(remove_instance);
    }

    static Stack<ImpactEffect> GetStack(ImpactEffect prefab)
    {
        if (!prefab)
            return null;
        if (!impact_effect_pool.TryGetValue(prefab, out Stack<ImpactEffect> stack))
        {
            stack = new Stack<ImpactEffect>();
            impact_effect_pool[prefab] = stack;
        }
        return stack;
    }

    // create a fresh ImpactEffect when one is not present
    static ImpactEffect Create(ImpactEffect prefab)
    {
        ImpactEffect instance = Instantiate(prefab, root).GetComponent<ImpactEffect>();
        
        instance.AbsolutePrefab = prefab;
        impact_effect_pool[prefab] = new Stack<ImpactEffect>();
        return instance;
    }
    #endregion
}
