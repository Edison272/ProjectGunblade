using AttackSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public enum AttackEnum { Projectile, Linecast, MeleeAttack }

/// Serialized attack config. Spawns instances of its prefab and hands each to its behavior script.
/// Subclasses only declare their spread settings and how to launch their behavior.
[System.Serializable]
public abstract class AttackObject
{
    [SerializeField] public GameObject instance; // The outward-facing property to set the instance in code
    [SerializeField, HideInInspector] protected AttackBehaviorBase _instance; // the actual instance field which will be set if the instance is accepted
    [SerializeField] public TargetFaction targetFaction;
    public AttackStats atk_stats;
    public abstract AttackTypeData TypeData {get;}

    public AttackObject(AttackBehaviorBase instance = null)
    {
        this._instance = instance;
    }
    /// Initialize the spawned instance's behavior.
    /// miss = 0..1, how far a randomly scattered shot strayed from the aim (always 0 for even spread)
    /// returns the newly-made instance
    protected abstract void Launch(GameObject spawned, TargetData shot);

    /// Override to pool instead of instantiate
    protected virtual AttackBehaviorBase Spawn(Vector2 position)
    {
        return AttackBehaviorPool.GetAttack(_instance, position);
    }

    public virtual TargetDataRequest GetTargetDataReq()
    {
        TargetDataRequest req = new TargetDataRequest();
        req.TargetFaction = targetFaction;
        return req;
    }

    #region Attack
    public virtual void Attack(TargetData atk_targ) => Attack(atk_targ, null);
    public virtual void Attack(TargetData atk_targ, List<AttackBehaviorBase> new_attacks)
    {
        Vector2 toTarget = atk_targ.targetPos - atk_targ.sourcePos;
        float baseAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

        for (int i = 0; i < TypeData.Count; i++)
        {
            Vector2 target = ShotTarget(atk_targ, toTarget.magnitude, baseAngle, i, TypeData);
            TargetData shot = atk_targ.WithTargetPos(target);
            AttackBehaviorBase new_attack = Spawn(shot.sourcePos);
            new_attacks?.Add(new_attack);
            Launch(new_attack.gameObject, shot);
        }
    }

    private static Vector2 ShotTarget(TargetData data, float dist, float baseAngle, int index, AttackTypeData atkTypeData)
    {
        float offset;
        if (atkTypeData.Even)
        {
            // fan spans the full angle; a single shot goes straight
            offset = atkTypeData.Count > 1
                ? -atkTypeData.Angle / 2f + atkTypeData.Angle / (atkTypeData.Count - 1) * index
                : 0f;
        }
        else
        {
            float half = atkTypeData.Angle / 2f;
            offset = Random.Range(-half, half);
        }

        float rad = (baseAngle + offset) * Mathf.Deg2Rad;
        return data.sourcePos + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * dist;
    }
    #endregion

    #region Recasting
    public virtual Projectile RecastToProjectileType() => new Projectile(_instance, atk_stats, new ProjectileTypeData());
    public virtual Linecast RecastToLinecastType() => new Linecast(_instance, atk_stats, new LinecastTypeData());
    public virtual MeleeAttack RecastToMeleeType() => new MeleeAttack(_instance, atk_stats, new MeleeTypeData());
    public virtual AreaEffect RecastToAreaEffectType() => new AreaEffect(_instance, atk_stats, new AreaEffectTypeData());

    public bool UpdateSerialization(out AttackObject result)
    {
        result = this;

        if (instance == null) { _instance = null; return false; }

        if (!instance.TryGetComponent(out AttackBehaviorBase behavior))
        {
            Debug.LogWarning($"'{instance.name}' has no AttackBehaviorBase. Rejected.");
            instance = _instance ? _instance.gameObject : null;
            return false;
        }

        _instance = behavior; // also resyncs after domain reload
        AttackObject recast = behavior switch
        {
            ProjectileBehavior when this is not Projectile => RecastToProjectileType(),
            LinecastBehavior   when this is not Linecast   => RecastToLinecastType(),
            MeleeBehavior      when this is not MeleeAttack => RecastToMeleeType(),
            AreaEffectBehavior when this is not AreaEffect => RecastToAreaEffectType(),
            _ => null
        };

        if (recast == null) return false;

        recast.instance = instance; // recast constructors only set _instance
        result = recast;
        Debug.Log($"{behavior}, {result}, {result.TypeData != null}");
        return true;
    }
    #endregion
    #region Custom Drawer
    [CustomPropertyDrawer(typeof(AttackObject), true)]
    public class AttackObjectDrawer : PropertyDrawer
    {
        static float Line => EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent l)
        {
            float height = Line;
            if (prop.propertyType != SerializedPropertyType.ManagedReference || prop.managedReferenceValue == null || !prop.isExpanded) return height;

            // Add to property height for all properties
            var it = prop.Copy(); 
            var end = prop.GetEndProperty();
            it.NextVisible(true);
            do { height += EditorGUI.GetPropertyHeight(it, true) + 2; }
            while (it.NextVisible(false) && !SerializedProperty.EqualContents(it, end));
            return height;
        }

        public override void OnGUI(Rect r, SerializedProperty prop, GUIContent guiContent)
        {
            var line = new Rect(r.x, r.y, r.width, Line);
            bool isNull = prop.managedReferenceValue == null;
            string typeName = isNull ? "None" : prop.managedReferenceValue.GetType().Name;

            var labelRect = new Rect(line.x, line.y, EditorGUIUtility.labelWidth, Line);
            var btnRect = new Rect(line.x + EditorGUIUtility.labelWidth, line.y,
                                line.width - EditorGUIUtility.labelWidth, Line);

            if (!isNull) prop.isExpanded = EditorGUI.Foldout(labelRect, prop.isExpanded, guiContent, true);
            else EditorGUI.LabelField(labelRect, guiContent);
            // create a drop down selector to instantiate the Attack Object
            if (EditorGUI.DropdownButton(btnRect, new GUIContent(typeName), FocusType.Keyboard))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("None"), isNull, () => { prop.managedReferenceValue = null; prop.serializedObject.ApplyModifiedProperties(); });
                foreach (var t in TypeCache.GetTypesDerivedFrom<AttackObject>().Where(t => !t.IsAbstract))
                {
                    var type = t;
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        prop.managedReferenceValue = Activator.CreateInstance(type, (object)null);
                        prop.isExpanded = true;
                        prop.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.DropDown(btnRect);
            }

            if (isNull || !prop.isExpanded) return;

            EditorGUI.indentLevel++;
            float y = r.y + Line + 2;
            var it = prop.Copy(); var end = prop.GetEndProperty();
            it.NextVisible(true);
            do
            {
                float h = EditorGUI.GetPropertyHeight(it, true);
                EditorGUI.PropertyField(new Rect(r.x, y, r.width, h), it, true);
                y += h + 2;
            } while (it.NextVisible(false) && !SerializedProperty.EqualContents(it, end));
            EditorGUI.indentLevel--;
        }
    }
    #endregion
}

#region Projectile
[System.Serializable]
public class Projectile : AttackObject
{
    public ProjectileTypeData typeData;
    public override AttackTypeData TypeData => typeData;

    public Projectile(AttackBehaviorBase instance = null) : base(instance) { }
    public Projectile(AttackBehaviorBase instance, AttackStats atk_stats, ProjectileTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    public override TargetDataRequest GetTargetDataReq()
    {
        TargetDataRequest req = base.GetTargetDataReq();
        req.HomingRadius = typeData.homing_radius;
        return req;
    }

    // scattered shots lose speed the further they miss (clamped to 50%)
    protected override void Launch(GameObject spawned, TargetData shot) =>
        spawned.GetComponent<ProjectileBehavior>().StartProjectile(this, shot);
}
#endregion
#region Linecast

[System.Serializable]
public class Linecast : AttackObject
{
    public LinecastTypeData typeData;
    public override AttackTypeData TypeData => typeData;

    public Linecast(AttackBehaviorBase instance = null) : base(instance) { }
    public Linecast(AttackBehaviorBase instance, AttackStats atk_stats, LinecastTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override void Launch(GameObject spawned, TargetData shot) =>
        spawned.GetComponent<LinecastBehavior>().StartLinecast(this, shot);
}
#endregion
#region MeleeAttack
[System.Serializable]
public class MeleeAttack : AttackObject
{
    public MeleeTypeData typeData;
    public override AttackTypeData TypeData => typeData;

    public MeleeAttack(AttackBehaviorBase instance = null) : base(instance) { }
    public MeleeAttack(AttackBehaviorBase instance, AttackStats atk_stats, MeleeTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override void Launch(GameObject spawned, TargetData shot) =>
        spawned.GetComponent<MeleeBehavior>().StartMelee(this, shot);
}
#endregion
#region AreaEffect
[System.Serializable]
public class AreaEffect : AttackObject
{
    public AreaEffectTypeData typeData;
    public override AttackTypeData TypeData => typeData;
    public AreaEffect(AttackBehaviorBase instance = null) : base(instance) { }
    public AreaEffect(AttackBehaviorBase instance, AttackStats atk_stats, AreaEffectTypeData typeData) : base(instance)
    {
        this.atk_stats = atk_stats;
        this.typeData = typeData;
    }

    protected override void Launch(GameObject spawned, TargetData shot) =>
        spawned.GetComponent<AreaEffectBehavior>().StartAreaEffect(this, shot);
}
#endregion
