using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute conditional = (ConditionalHideAttribute)attribute;
        SerializedProperty conditionProperty = FindConditionProperty(property, conditional.ConditionName);

        if (conditionProperty != null && ShouldShowProperty(conditional, conditionProperty))
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute conditional = (ConditionalHideAttribute)attribute;
        SerializedProperty conditionProperty = FindConditionProperty(property, conditional.ConditionName);

        if (conditionProperty != null && ShouldShowProperty(conditional, conditionProperty))
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return 0f; // Hide the property if conditions are not met
        }
    }

    private SerializedProperty FindConditionProperty(SerializedProperty property, string conditionName)
    {
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionName);
        if (conditionProperty != null)
        {
            return conditionProperty;
        }

        Debug.LogWarning($"Could not find condition property '{conditionName}'");
        return null;
    }


    private bool ShouldShowProperty(ConditionalHideAttribute conditional, SerializedProperty conditionProperty)
    {
        if (conditionProperty == null)
        {
            Debug.LogWarning($"Could not find condition property for {conditional.ConditionName}");
            return true; // Default to showing property if condition property not found
        }

        bool conditionMet = false;

        // Check the type of the condition property and compare with target value
        switch (conditionProperty.propertyType)
        {
            case SerializedPropertyType.Boolean:
                conditionMet = conditionProperty.boolValue == (conditional.TargetValue == 1);
                break;
            case SerializedPropertyType.Enum:
                conditionMet = conditionProperty.enumValueIndex == conditional.TargetValue;
                break;
            case SerializedPropertyType.Integer:
                conditionMet = conditionProperty.intValue == conditional.TargetValue;
                break;
            default:
                Debug.LogWarning($"Unsupported property type: {conditionProperty.propertyType} for {conditional.ConditionName}");
                break;
        }

        return conditionMet;
    }

    private T GetCustomAttribute<T>(SerializedProperty property) where T : PropertyAttribute
    {
        FieldInfo fieldInfo = property.serializedObject.targetObject.GetType().GetField(property.propertyPath);
        if (fieldInfo != null)
        {
            return (T)System.Attribute.GetCustomAttribute(fieldInfo, typeof(T));
        }
        return null;
    }
}
