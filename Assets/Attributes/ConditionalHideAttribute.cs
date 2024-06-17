using UnityEngine;

/// <summary>
/// Shows/Hides serialized fields based on a condition bool or enum state.
/// </summary>
public class ConditionalHideAttribute : PropertyAttribute
{
    public string ConditionName { get; private set; }
    public int[] TargetValues { get; private set; }

    public ConditionalHideAttribute(string conditionName, params int[] targetValues)
    {
        ConditionName = conditionName;
        TargetValues = targetValues;
    }
}
