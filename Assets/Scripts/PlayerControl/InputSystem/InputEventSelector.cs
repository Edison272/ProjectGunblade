using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;


public enum InputEventType {
    None = -1,
    Global, 
    Character, 
    Usable,
};
/// used to contain multiple enums from InputEvents.cs
/// - declutter inspectors so that I don't have to scroll through a list of 1000000 enums of a single type pick the event type, and then the specific event type
/// - Hopefully a reflection-free way to get enum data
[Serializable]
public class InputEventSelector
{
    public InputEventType InputEventType = InputEventType.Character;
    public Enum InputEvent => GetInputEvent();
    [SerializeField] private GlobalEvent _globalInput = GlobalEvent.Update;
    [SerializeField] private CharacterEvent _characterInput = CharacterEvent.MoveStart;
    [SerializeField] private UsableEvent _usableInput = UsableEvent.Equip;

    public InputEventSelector() {}
    public InputEventSelector(Enum inputEvent)
    {
        SetInputEvent(inputEvent);
    }

    #region SetInputEvent Overrides
    public void SetInputEvent(Enum inputEvent)
    {
        switch (inputEvent)
        {
            case GlobalEvent global:
                SetInputEvent((GlobalEvent)inputEvent);
                break;
            case CharacterEvent character:
                SetInputEvent((CharacterEvent)inputEvent);
                break;
            case UsableEvent usable:
                SetInputEvent((UsableEvent)inputEvent);
                break;
            default:
                Debug.LogWarning($"Unsupported enum type: {inputEvent.GetType()}");
                break;
        }
    }
    public void SetInputEvent(GlobalEvent value)
    {
        _globalInput = value;
        InputEventType = InputEventType.Global;
    }

    public void SetInputEvent(CharacterEvent value)
    {
        _characterInput = value;
        InputEventType = InputEventType.Character;
    }

    public void SetInputEvent(UsableEvent value)
    {
        _usableInput = value;
        InputEventType = InputEventType.Usable;
    }
    #endregion

    public Type GetInputEventType()
    {
        switch (InputEventType)
        {
            case InputEventType.Global: return typeof(GlobalEvent);
            case InputEventType.Character: return typeof(CharacterEvent);
            case InputEventType.Usable: return typeof(UsableEvent);
        }
        return null; // fails if it reaches here. shouldn't be possible tho.
    }

    public Enum GetInputEvent()
    {
        switch (InputEventType)
        {
            case InputEventType.Global: return _globalInput;
            case InputEventType.Character: return _characterInput;
            case InputEventType.Usable: return _usableInput;
        }
        return null; // fails if it reaches here. shouldn't be possible tho.
    }


    #region Static Helpers

    // check if input type exists here

    // Get Input Event Type from any InputEvent Enum
    public static InputEventType GetInputEventTypeFromEnum(Enum inputEvent)
    {
        switch (inputEvent)
        {
            case GlobalEvent global:
                return InputEventType.Global;
            case CharacterEvent character:
                return InputEventType.Character;
            case UsableEvent usable:
                return InputEventType.Usable;
            default:
                Debug.LogWarning($"Unsupported enum type: {inputEvent.GetType()}");
                return InputEventType.None;
        }
    }

    public static Type GetTypeFromEnum(Enum inputEvent)
    {
        switch (inputEvent)
        {
            case GlobalEvent global:
                return typeof(GlobalEvent);
            case CharacterEvent character:
                return typeof(CharacterEvent);
            case UsableEvent usable:
                return typeof(UsableEvent);
            default:
                Debug.LogWarning($"Unsupported enum type: {inputEvent.GetType()}");
                return null;
        }
    }
    public static int GetEnumLength(Enum inputEvent)
    {
        return Enum.GetValues(InputEventSelector.GetTypeFromEnum(inputEvent)).Length;
    }
    public static Type GetTypeFromInt(int eventInt)
    {
        switch ((InputEventType)eventInt)
        {
            case InputEventType.Global:
                return typeof(GlobalEvent);
            case InputEventType.Character:
                return typeof(CharacterEvent);
            case InputEventType.Usable:
                return typeof(UsableEvent);
            default:
                Debug.LogError($"{eventInt} int cannot be converted because it is out of range!");
                return null;
        }
    }
    public static Enum GetEnumFromTypeInt(InputEventType eventType, int eventInt)
    {
        switch (eventType)
        {
            case InputEventType.Global:
                return (GlobalEvent)eventInt;
            case InputEventType.Character:
                return (CharacterEvent)eventInt;
            case InputEventType.Usable:
                return (UsableEvent)eventInt;
            default:
                Debug.LogError($" {eventType} {eventInt} int cannot be converted because it is out of range!");
                return null;
        }
    }

    #endregion
}



// GUI stuff to make this much cleaner in the inspector
#region Custom GUI

[CustomPropertyDrawer(typeof(InputEventSelector))]
public class MyCustomDataDrawer : PropertyDrawer 
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
    {
        // Begin property check to handle Unity's undo/redo system safely
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty typeProp = property.FindPropertyRelative("InputEventType");

        EditorGUI.BeginChangeCheck();

        EditorGUI.LabelField(position, property.displayName);

        Rect typeRect = new Rect(position.x + position.width * 0.5f, position.y, position.width * 0.25f, EditorGUIUtility.singleLineHeight);
        Rect enumRect = new Rect(position.x + position.width * 0.75f, position.y, position.width * 0.25f, EditorGUIUtility.singleLineHeight);

        EditorGUI.PropertyField(typeRect, typeProp, GUIContent.none);

        // Find the concrete backing field by matching its Type, not its name
        object dataHolder = GetTargetObjectOfProperty(property);
        Type activeEnumType = GetActiveEnumType(dataHolder);
        string concreteFieldName = FindFieldNameByType(dataHolder, activeEnumType);

        if (concreteFieldName != null)
        {
            SerializedProperty concreteProp = property.FindPropertyRelative(concreteFieldName);
            EditorGUI.PropertyField(enumRect, concreteProp, GUIContent.none);
        }


        EditorGUI.EndProperty();
    }

    // Calls GetInputEventType() via reflection so the drawer doesn't need
    // a hard reference to InputEventSelector's concrete method signature.
    private Type GetActiveEnumType(object dataHolder)
    {
        if (dataHolder == null) return null;
        MethodInfo method = dataHolder.GetType().GetMethod("GetInputEventType");
        return method?.Invoke(dataHolder, null) as Type;
    }

    // Scans the object's fields and returns the name of the first field
    // whose declared type matches targetType.
    private string FindFieldNameByType(object dataHolder, Type targetType)
    {
        if (dataHolder == null || targetType == null) return null;

        FieldInfo[] fields = dataHolder.GetType().GetFields(
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
        );

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == targetType)
                return field.Name;
        }
        return null;
    }

    public static object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        string path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        string[] elements = path.Split('.');

        foreach (string element in elements)
        {
            if (element.Contains("["))
            {
                string elementName = element.Substring(0, element.IndexOf("["));
                int index = Convert.ToInt32(
                    element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", "")
                );
                obj = GetValue(obj, elementName, index);
            }
            else
            {
                obj = GetValue(obj, element);
            }
        }
        return obj;
    }

    private static object GetValue(object source, string name)
    {
        if (source == null) return null;
        Type type = source.GetType();
        while (type != null)
        {
            FieldInfo field = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(source);

            PropertyInfo property = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (property != null) return property.GetValue(source, null);

            type = type.BaseType;
        }
        return null;
    }

    private static object GetValue(object source, string name, int index)
    {
        var enumerable = GetValue(source, name) as System.Collections.IEnumerable;
        if (enumerable == null) return null;

        var enumerator = enumerable.GetEnumerator();
        for (int i = 0; i <= index; i++)
        {
            if (!enumerator.MoveNext()) return null;
        }
        return enumerator.Current;
    }
}
#endregion