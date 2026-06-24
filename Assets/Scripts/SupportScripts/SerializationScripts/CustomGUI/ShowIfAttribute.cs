using UnityEditor;
using UnityEngine;

// Attribute usage:
//   [ShowIf("isEnabled")]                          -> shown when isEnabled == true (bool shorthand)
//   [ShowIf("mode", MyEnum.Special)]                -> shown when mode == MyEnum.Special
//   [ShowIf("mode", MyEnum.Special, "Special Cfg")] -> same, but field is relabeled "Special Cfg" in inspector
public class ShowIfAttribute : PropertyAttribute
{
    public string condition_field;
    public object condition_value;
    public string display_name;

    // original bool version
    public ShowIfAttribute(string condition_field, string display_name = null)
    {
        this.condition_field = condition_field;
        this.condition_value = true;
        this.display_name = display_name;
        
    }

    // value comparison version
    public ShowIfAttribute(string condition_field, object condition_value, string display_name = null)
    {
        this.condition_field = condition_field;
        this.condition_value = condition_value;
        this.display_name = display_name;
    }

}

// General constructor: show this field when condition_field equals condition_value
// (supports bool/int/float/enum, per IsConditionMet below).
[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute show_if = (ShowIfAttribute)attribute;
        string condition_path = property.propertyPath.Replace(property.name, show_if.condition_field);
        SerializedProperty condition_prop = property.serializedObject.FindProperty(condition_path);

        if (condition_prop != null && IsConditionMet(condition_prop, show_if.condition_value)) {
            GUIContent display_label = show_if.display_name != null 
                ? new GUIContent(show_if.display_name) 
                : label;
            EditorGUI.PropertyField(position, property, display_label, true);
        }
    }

    bool IsConditionMet(SerializedProperty prop, object value)
    {
        switch (prop.propertyType)
        {
            case SerializedPropertyType.Boolean:
                return prop.boolValue == (bool)value;
            case SerializedPropertyType.Integer:
                return prop.intValue == (int)value;
            case SerializedPropertyType.Float:
                return prop.floatValue == (float)value;
            case SerializedPropertyType.Enum:
                return prop.enumValueIndex == (int)value;
            case SerializedPropertyType.ObjectReference:
                return prop.objectReferenceValue != null;
            default:
                return false;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute show_if = (ShowIfAttribute)attribute;
        string condition_path = property.propertyPath.Replace(property.name, show_if.condition_field);

        SerializedProperty condition_prop = property.serializedObject.FindProperty(condition_path);
        if (condition_prop != null && IsConditionMet(condition_prop, show_if.condition_value))
            return EditorGUI.GetPropertyHeight(property, label, true) + 1;
        
        return 0; //-EditorGUIUtility.standardVerticalSpacing; // use defaul spacing when property is hidden
    }
}