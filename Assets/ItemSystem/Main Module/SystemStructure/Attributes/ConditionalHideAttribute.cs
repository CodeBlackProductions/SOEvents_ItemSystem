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
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class ConditionalHideAttribute : Attribute
    {
        public string ConditionName { get; private set; }
        public int[] TargetValues { get; private set; }
        public bool TargetBool { get; private set; }

        public ConditionalHideAttribute(string _ConditionName, params int[] _TargetValues)
        {
            ConditionName = _ConditionName;
            TargetValues = _TargetValues;
        }

        public ConditionalHideAttribute(string _ConditionName, bool _TargetBool)
        {
            ConditionName = _ConditionName;
            TargetBool = _TargetBool;
        }

        public static bool ShouldShowProperty(ScriptableObject _Target, PropertyInfo _Property)
        {
            if (_Target == null || _Property == null)
            {
                Debug.LogWarning("Target or property is null.");
                return true;
            }

            var conditional = _Property.GetCustomAttribute<ConditionalHideAttribute>();
            if (conditional == null)
            {
                return true;
            }

            PropertyInfo conditionProperty = _Target.GetType().GetProperty(conditional.ConditionName, BindingFlags.Public | BindingFlags.Instance);

            object conditionValue = conditionProperty?.GetValue(_Target);
            if (conditionValue == null)
            {
                Debug.LogWarning($"Condition property '{conditional.ConditionName}' not found on {_Target.GetType().Name}");
                return true;
            }

            return EvaluateCondition(conditional, conditionValue);
        }

        private static bool EvaluateCondition(ConditionalHideAttribute _Conditional, object _ConditionValue)
        {
            if (_ConditionValue is bool boolValue)
            {
                return _Conditional.TargetBool.Equals(boolValue);
            }
            if (_ConditionValue is int intValue)
            {
                return _Conditional.TargetValues.Contains(intValue);
            }
            if (_ConditionValue is Enum enumValue)
            {
                return _Conditional.TargetValues.Contains(Convert.ToInt32(enumValue));
            }

            Debug.LogWarning($"Unsupported condition type: {_ConditionValue.GetType()} for {_Conditional.ConditionName}");
            return true;
        }
    }
}