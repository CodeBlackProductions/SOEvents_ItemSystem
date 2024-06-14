using UnityEngine;

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
