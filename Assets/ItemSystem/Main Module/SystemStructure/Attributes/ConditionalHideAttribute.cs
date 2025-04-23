using System;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace ItemSystem.Editor
{
    /// <summary>
    /// Shows/Hides serialized fields based on a condition bool or enum state.
    /// </summary>
    public class ConditionalHideAttribute : Attribute
    {
        public string m_ConditionName { get; private set; }
        public int[] m_TargetValues { get; private set; }

        public ConditionalHideAttribute(string _ConditionName, params int[] _TargetValues)
        {
            m_ConditionName = _ConditionName;
            m_TargetValues = _TargetValues;
        }

        public static bool ShouldShowProperty(ScriptableObject _Target, PropertyInfo _Property)
        {
            if (_Target == null || _Property == null)
            {
                Debug.LogWarning("Target or property is null.");
                return true;
            }

            // Get the ConditionalHideAttribute
            var conditional = _Property.GetCustomAttribute<ConditionalHideAttribute>();
            if (conditional == null)
            {
                return true; // No condition, so always show
            }

            PropertyInfo conditionProperty = _Target.GetType().GetProperty(conditional.m_ConditionName, BindingFlags.Public | BindingFlags.Instance);

            object conditionValue = conditionProperty?.GetValue(_Target);
            if (conditionValue == null)
            {
                Debug.LogWarning($"Condition property '{conditional.m_ConditionName}' not found on {_Target.GetType().Name}");
                return true;
            }

            // Evaluate the condition
            return EvaluateCondition(conditional, conditionValue);
        }

        private static bool EvaluateCondition(ConditionalHideAttribute _Conditional, object _ConditionValue)
        {
            if (_ConditionValue is bool boolValue)
            {
                return _Conditional.m_TargetValues.Contains(boolValue ? 1 : 0);
            }
            if (_ConditionValue is int intValue)
            {
                return _Conditional.m_TargetValues.Contains(intValue);
            }
            if (_ConditionValue is Enum enumValue)
            {
                return _Conditional.m_TargetValues.Contains(Convert.ToInt32(enumValue));
            }

            Debug.LogWarning($"Unsupported condition type: {_ConditionValue.GetType()} for {_Conditional.m_ConditionName}");
            return true;
        }
    }
}