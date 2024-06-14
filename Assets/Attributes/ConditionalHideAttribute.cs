using UnityEngine;

public class ConditionalHideAttribute : PropertyAttribute
{
    public string ConditionName { get; private set; }
    public int TargetValue { get; private set; }

    public ConditionalHideAttribute(string conditionName, int targetValue)
    {
        ConditionName = conditionName;
        TargetValue = targetValue;
    }
}
