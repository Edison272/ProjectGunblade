using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Attribute usage:
//   [ShowIfMethod("IsAdvancedMode")]   -> shown when IsAdvancedMode() returns true
//
// Method must be parameterless and return bool. Can be private, public,
// static or instance, and can contain any logic you want -- this is the
// flexible equivalent of passing a lambda, which attributes can't accept directly.
//
//   private bool IsAdvancedMode() => mode == Mode.Advanced && level > 3;
//   [ShowIfMethod("IsAdvancedMode")]
//   public float advancedSetting;
public class ShowIfMethodAttribute : PropertyAttribute
{
    public string method_name;
    public string display_name;

    public ShowIfMethodAttribute(string method_name, string display_name = null)
    {
        this.method_name = method_name;
        this.display_name = display_name;
    }
}

[CustomPropertyDrawer(typeof(ShowIfMethodAttribute))]
public class ShowIfMethodDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfMethodAttribute show_if = (ShowIfMethodAttribute)attribute;

        if (EvaluateMethod(property, show_if.method_name))
        {
            GUIContent display_label = show_if.display_name != null
                ? new GUIContent(show_if.display_name)
                : label;
            EditorGUI.PropertyField(position, property, display_label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfMethodAttribute show_if = (ShowIfMethodAttribute)attribute;

        if (EvaluateMethod(property, show_if.method_name))
            return EditorGUI.GetPropertyHeight(property, label, true) + 1;

        return 0;
    }

    bool EvaluateMethod(SerializedProperty property, string method_name)
    {
        // Resolve the object that actually OWNS this property -- could be the
        // root ScriptableObject/MonoBehaviour, or a nested [SerializeReference]/
        // [System.Serializable] instance like Projectile.
        object owner = GetParentObject(property);
        if (owner == null)
        {
            Debug.LogWarning($"ShowIfMethod: could not resolve owning object for '{property.propertyPath}'.");
            return true;
        }

        // Walk up the inheritance chain too -- handles methods declared on a
        // base class (e.g. a shared Validate-style check on AttackType
        // rather than redeclared on every subclass).
        Type type = owner.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo method = null;
        while (type != null && method == null)
        {
            method = type.GetMethod(method_name, flags, null, Type.EmptyTypes, null);
            type = type.BaseType;
        }

        if (method == null || method.ReturnType != typeof(bool))
        {
            Debug.LogWarning($"ShowIfMethod: no parameterless bool method named '{method_name}' found on {owner.GetType().Name} or its base types.");
            return true; // fail open, same reasoning as before
        }

        return (bool)method.Invoke(owner, null);
    }

    // Walks propertyPath segment by segment via reflection, resolving the actual
    // object instance at each step, to find the object that directly contains
    // the attributed property -- not just the root serialized object.
    object GetParentObject(SerializedProperty property)
    {
        string path = property.propertyPath.Replace(".Array.data[", "[");
        object obj = property.serializedObject.targetObject;
        string[] elements = path.Split('.');

        // Stop one short of the last element -- that last element is the
        // property itself, we want what's holding it, not its own value.
        for (int i = 0; i < elements.Length - 1; i++)
        {
            string element = elements[i];
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = int.Parse(element.Substring(element.IndexOf("[") + 1).TrimEnd(']'));
                obj = GetFieldValue(GetFieldValue(obj, elementName) as IList, index);
            }
            else
            {
                obj = GetFieldValue(obj, element);
            }

            if (obj == null) return null;
        }

        return obj;
    }

    object GetFieldValue(object source, string name)
    {
        if (source == null) return null;
        Type type = source.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        // Fields can be declared on a base class (e.g. "instance" on AttackType
        // rather than Projectile) -- walk up until found.
        while (type != null)
        {
            FieldInfo field = type.GetField(name, flags);
            if (field != null) return field.GetValue(source);
            type = type.BaseType;
        }
        return null;
    }

    object GetFieldValue(IList list, int index)
    {
        if (list == null || index < 0 || index >= list.Count) return null;
        return list[index];
    }
}