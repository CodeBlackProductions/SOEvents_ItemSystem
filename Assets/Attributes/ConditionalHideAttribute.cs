using System.Linq;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;

/// <summary>
/// Shows/Hides serialized fields based on a condition bool or enum state.
/// </summary>
public class ConditionalHideAttribute : Attribute
{
    public string ConditionName { get; private set; }
    public int[] TargetValues { get; private set; }

    public ConditionalHideAttribute(string conditionName, params int[] targetValues)
    {
        ConditionName = conditionName;
        TargetValues = targetValues;
    }

    public static bool ShouldShowProperty(ScriptableObject target, PropertyInfo property)
    {
        if (target == null || property == null)
        {
            Debug.LogWarning("Target or property is null.");
            return true;
        }

        // Get the ConditionalHideAttribute
        var conditional = property.GetCustomAttribute<ConditionalHideAttribute>();
        if (conditional == null)
        {
            return true; // No condition, so always show
        }

        PropertyInfo conditionProperty = target.GetType().GetProperty(conditional.ConditionName, BindingFlags.Public | BindingFlags.Instance);

        object conditionValue = conditionProperty?.GetValue(target);
        if (conditionValue == null)
        {
            Debug.LogWarning($"Condition property '{conditional.ConditionName}' not found on {target.GetType().Name}");
            return true;
        }

        // Evaluate the condition
        return EvaluateCondition(conditional, conditionValue);
    }

    private static bool EvaluateCondition(ConditionalHideAttribute conditional, object conditionValue)
    {
        if (conditionValue is bool boolValue)
        {
            return conditional.TargetValues.Contains(boolValue ? 1 : 0);
        }
        if (conditionValue is int intValue)
        {
            return conditional.TargetValues.Contains(intValue);
        }
        if (conditionValue is Enum enumValue)
        {
            return conditional.TargetValues.Contains(Convert.ToInt32(enumValue));
        }

        Debug.LogWarning($"Unsupported condition type: {conditionValue.GetType()} for {conditional.ConditionName}");
        return true;
    }
}